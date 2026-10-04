#!/bin/sh
# Usage: eng/sync-from-upstream.sh <clone of testcontainers/testcontainers-dotnet> <ref>
# Copies the Chroma module of testcontainers-dotnet at <ref> (the branch of the pull request, fetched first, or develop
# once it is merged) into this repository and commits it, with a Co-authored-by line for each author of the upstream
# commits since the last sync. The last synced upstream commit is kept in eng/upstream-commit.txt.
set -e
upstream="$1"; ref="$2"
[ -n "$upstream" ] && [ -n "$ref" ] || { echo "Usage: $0 <clone of testcontainers/testcontainers-dotnet> <ref>" >&2; exit 2; }
repo=$(git rev-parse --show-toplevel)
slug=testcontainers/testcontainers-dotnet
paths="src/Testcontainers.Chroma tests/Testcontainers.Chroma.Tests docs/modules/chroma.md"

new=$(git -C "$upstream" rev-parse "$ref")
old=$(cat "$repo/eng/upstream-commit.txt" 2>/dev/null || true)
[ "$new" = "$old" ] && { echo "Already at $new."; exit 0; }

# The module of testcontainers-dotnet replaces ours, deleted files included.
for p in $paths; do
  git -C "$repo" rm -rq --ignore-unmatch "$p"
  git -C "$upstream" archive "$new" "$p" | tar -x -C "$repo"
done
echo "$new" > "$repo/eng/upstream-commit.txt"
git -C "$repo" add -A $paths eng/upstream-commit.txt

if git -C "$repo" diff --cached --quiet -- $paths; then
  echo "No change in the module."
  git -C "$repo" commit -q -m "Record $slug $(echo "$new" | cut -c1-8) as synced"
  exit 0
fi

# Authors of the upstream commits since the last sync that touch the module, as GitHub noreply addresses when the
# commit carries a login, never other emails.
coauthors=""
if [ -n "$old" ] && git -C "$upstream" merge-base --is-ancestor "$old" "$new" 2>/dev/null; then
  range="$old..$new"
else
  range="$new -1"
fi
me=$(gh api user --jq .login)
for sha in $(git -C "$upstream" rev-list $range -- $paths); do
  login=$(gh api "repos/$slug/commits/$sha" --jq '.author.login // empty' 2>/dev/null || true)
  [ -z "$login" ] || [ "$login" = "$me" ] && continue
  id=$(gh api "users/$login" --jq .id)
  name=$(git -C "$upstream" log -1 --format=%an "$sha" | cut -d' ' -f1)
  line="Co-authored-by: $name <$id+$login@users.noreply.github.com>"
  case "$coauthors" in *"$line"*) ;; *) coauthors="$coauthors
$line";; esac
done

git -C "$repo" commit -q -F - <<EOF
Sync with $slug $(echo "$new" | cut -c1-8)
$coauthors
EOF
git -C "$repo" log -1 --stat | cat
