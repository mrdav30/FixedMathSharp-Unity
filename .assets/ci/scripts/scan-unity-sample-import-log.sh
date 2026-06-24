#!/usr/bin/env bash
set -euo pipefail

if [[ $# -ne 1 ]]; then
  echo "Usage: scan-unity-sample-import-log.sh <editmode-log-path>" >&2
  exit 2
fi

log_file="$1"
if [[ ! -f "$log_file" ]]; then
  echo "Unity editmode log was not found: $log_file" >&2
  artifact_dir="$(dirname "$log_file")"
  find "$artifact_dir" -maxdepth 3 -type f -print 2>/dev/null || true
  exit 1
fi

if ! grep -F "FMS_SAMPLE_SMOKE imported sample" "$log_file" > /dev/null; then
  echo "Sample import marker was not found in $log_file" >&2
  exit 1
fi

bad_patterns=(
  "A meta data file (.meta) exists but its folder"
  "Assembly with name"
  "conflicting GUID"
  "DirectoryNotFoundException"
  "Samples~.meta"
  "Package add failed"
  "Scripts have compiler errors"
  "Compilation failed"
  "error CS"
)

found_bad_pattern=0
for pattern in "${bad_patterns[@]}"; do
  if grep -F -n "$pattern" "$log_file"; then
    found_bad_pattern=1
  fi
done

if [[ "$found_bad_pattern" -ne 0 ]]; then
  echo "Known sample import failure pattern found in $log_file" >&2
  exit 1
fi
