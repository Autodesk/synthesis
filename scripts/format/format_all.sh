#!/bin/bash

FILES_DIRS=("engine/Assets/Scripts")
FILE_TARGETS=(".cs")
FORMAT_COMMAND="clang-format -i -style=file"

if [[ ! -f ".clang-format" ]]; then
    echo "Error: .clang-format not found. Are you in the root of the project?"
    exit 1
fi

if ! command -v clang-format > /dev/null; then
    if [[ "$OSTYPE" == "linux-gnu" ]]; then
        echo "Error: clang-format not installed. Run 'sudo apt-get install clang-format' to install it."
    else
        echo "Error: clang-format not installed. Run 'brew install clang-format' to install it."
    fi
    exit 1
fi

files=()
for dir in "${FILES_DIRS[@]}"; do
    while IFS= read -r -d '' file; do
        if [[ "${file##*.}" = "cs" ]]; then
            files+=("$file")
        fi
    done < <(find "$dir" -type f -name "*.cs" -print0)
done

echo "Found ${#files[@]} files."

for file in "${files[@]}"; do
    echo "Formatting $file..."
    $FORMAT_COMMAND "$file"
done

echo "Done!"
