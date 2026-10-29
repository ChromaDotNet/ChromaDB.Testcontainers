#!/bin/sh
# Usage: eng/sync-from-upstream.sh <clone of testcontainers/testcontainers-dotnet> <ref>
# Copies the Chroma module of testcontainers-dotnet at <ref> (the branch of the pull request, fetched first, or develop
# once it is merged) into this repository and commits it. The commit is ours, so it names no upstream author: their
# names stay on their own commits. The last synced upstream commit is kept in eng/upstream-commit.txt.
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

git -C "$repo" commit -q -m "Sync with $slug $(echo "$new" | cut -c1-8)"
git -C "$repo" log -1 --stat | cat
