#!/usr/bin/env bash
set -euo pipefail

usage() {
  cat <<'EOF'
Usage: prepare-unity-sample-import-smoke.sh \
  --project-path <path> \
  --package-name <name> \
  --package-path <path> \
  --expected-sample-asmdef <file-name> \
  --workspace-path <path-visible-to-unity> \
  --git-sha <sha>
EOF
}

project_path=""
package_name=""
package_path=""
expected_sample_asmdef=""
workspace_path=""
git_sha=""

while [[ $# -gt 0 ]]; do
  case "$1" in
    --project-path)
      project_path="$2"
      shift 2
      ;;
    --package-name)
      package_name="$2"
      shift 2
      ;;
    --package-path)
      package_path="$2"
      shift 2
      ;;
    --expected-sample-asmdef)
      expected_sample_asmdef="$2"
      shift 2
      ;;
    --workspace-path)
      workspace_path="$2"
      shift 2
      ;;
    --git-sha)
      git_sha="$2"
      shift 2
      ;;
    -h|--help)
      usage
      exit 0
      ;;
    *)
      echo "Unknown argument: $1" >&2
      usage >&2
      exit 2
      ;;
  esac
done

if [[ -z "$project_path" || -z "$package_name" || -z "$package_path" || -z "$expected_sample_asmdef" || -z "$workspace_path" || -z "$git_sha" ]]; then
  usage >&2
  exit 2
fi

script_dir="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
repo_root="$(cd "$script_dir/../../.." && pwd)"
template_root="$repo_root/.assets/ci/unity-sample-import-smoke"

if [[ ! -d "$template_root" ]]; then
  echo "Unity sample import smoke template was not found: $template_root" >&2
  exit 1
fi

rm -rf "$project_path"
mkdir -p "$project_path"
cp -R "$template_root/." "$project_path/"

if [[ "$workspace_path" == /* ]]; then
  package_url="git+file://${workspace_path}?path=/${package_path}#${git_sha}"
else
  package_url="git+file:///${workspace_path}?path=/${package_path}#${git_sha}"
fi

escape_sed_replacement() {
  printf '%s' "$1" | sed -e 's/[\/&|]/\\&/g'
}

sed \
  -e "s|__PACKAGE_NAME__|$(escape_sed_replacement "$package_name")|g" \
  -e "s|__PACKAGE_URL__|$(escape_sed_replacement "$package_url")|g" \
  "$project_path/Packages/manifest.json.template" > "$project_path/Packages/manifest.json"

rm "$project_path/Packages/manifest.json.template"

sed \
  -e "s|__PACKAGE_NAME__|$(escape_sed_replacement "$package_name")|g" \
  -e "s|__EXPECTED_SAMPLE_ASMDEF__|$(escape_sed_replacement "$expected_sample_asmdef")|g" \
  "$project_path/Assets/FixedMathSharpSampleImportSmokeConfig.json.template" > "$project_path/Assets/FixedMathSharpSampleImportSmokeConfig.json"

rm "$project_path/Assets/FixedMathSharpSampleImportSmokeConfig.json.template"
