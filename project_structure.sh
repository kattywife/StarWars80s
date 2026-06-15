export LANG=C.UTF-8
export LC_ALL=C.UTF-8

#1. Create a fresh markdown file with the structure (using ASCII lines and raw Cyrillic)
echo "# Project Structure" > project_source.md
echo "\`\`\`text" >> project_source.md
tree --charset=ascii -N Assets -I "*.meta" --dirsfirst >> project_source.md
echo "\`\`\`" >> project_source.md
echo -e "\n---\n" >> project_source.md

# 2. Append each C# file (Cyrillic filenames and comments will be preserved as UTF-8)
find Assets -name "*.cs" -type f | while read -r file; do
    echo "# File: $file" >> project_source.md
    echo "\`\`\`csharp" >> project_source.md
    cat "$file" >> project_source.md
    echo -e "\n\`\`\`\n" >> project_source.md
done
