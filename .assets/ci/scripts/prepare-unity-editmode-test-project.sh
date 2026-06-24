#!/usr/bin/env bash
set -euo pipefail

usage() {
  cat <<'EOF'
Usage: prepare-unity-editmode-test-project.sh \
  --project-path <path> \
  --package-name <name> \
  --package-path <path> \
  --runtime-assembly <assembly-name>
EOF
}

project_path=""
package_name=""
package_path=""
runtime_assembly=""

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
    --runtime-assembly)
      runtime_assembly="$2"
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

if [[ -z "$project_path" || -z "$package_name" || -z "$package_path" || -z "$runtime_assembly" ]]; then
  usage >&2
  exit 2
fi

script_dir="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
repo_root="$(cd "$script_dir/../../.." && pwd)"
template_root="$repo_root/.assets/ci/unity-editmode-tests"

if [[ ! -d "$template_root" ]]; then
  echo "Unity EditMode test template was not found: $template_root" >&2
  exit 1
fi

rm -rf "$project_path"
mkdir -p "$project_path"
cp -R "$template_root/." "$project_path/"

mkdir -p "$project_path/Assets/Tests/EditMode"
mkdir -p "$project_path/Assets/Tests/Runtime"
cp "$repo_root"/Tests/EditMode/*.cs "$project_path/Assets/Tests/EditMode/"
cp "$repo_root"/Tests/Runtime/*.cs "$project_path/Assets/Tests/Runtime/"

escape_sed_replacement() {
  printf '%s' "$1" | sed -e 's/[\/&|]/\\&/g'
}

render_template() {
  local template_path="$1"
  local output_path="${template_path%.template}"

  sed \
    -e "s|__PACKAGE_NAME__|$(escape_sed_replacement "$package_name")|g" \
    -e "s|__PACKAGE_PATH__|$(escape_sed_replacement "$package_path")|g" \
    -e "s|__RUNTIME_ASSEMBLY__|$(escape_sed_replacement "$runtime_assembly")|g" \
    "$template_path" > "$output_path"

  rm "$template_path"
}

render_template "$project_path/Assets/Tests/Runtime/FixedMathSharp.Unity.Tests.Runtime.asmdef.template"
render_template "$project_path/Assets/Tests/EditMode/FixedMathSharp.Unity.Tests.EditMode.asmdef.template"
render_template "$project_path/Packages/manifest.json.template"
