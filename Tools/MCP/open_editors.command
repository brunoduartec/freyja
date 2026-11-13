#!/bin/zsh
set -eu
project_root="$(cd "$(dirname "$0")/../.." && pwd)"
unity_version="$(sed -n 's/^m_EditorVersion: //p' "$project_root/Freyja/ProjectSettings/ProjectVersion.txt")"
unity_app="/Applications/Unity/Hub/Editor/$unity_version/Unity.app"
[[ -d "$unity_app" ]] || { print -u2 "Editor Unity não encontrado: $unity_app"; exit 1; }
open -a "$unity_app" --args -projectPath "$project_root/Freyja"
open -a Blender "$project_root/Source/Blender/Cais_Iteracao01.blend" --args --online-mode
