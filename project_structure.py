import os


def print_tree(dir_path, prefix="", out_file=None):
    if not os.path.exists(dir_path):
        return
    # Get directory contents, excluding .meta files, hidden files, and Unity cache
    contents = sorted(
        [
            f
            for f in os.listdir(dir_path)
            if not f.endswith(".meta")
            and not f.startswith(".")
            and f
            not in ["Library", "Temp", "Logs", "obj", "Build", "Builds", "UserSettings"]
        ]
    )

    # Sort so folders appear first
    contents.sort(key=lambda x: not os.path.isdir(os.path.join(dir_path, x)))

    for i, item in enumerate(contents):
        path = os.path.join(dir_path, item)
        is_last = i == len(contents) - 1
        connector = "`-- " if is_last else "|-- "

        line = f"{prefix}{connector}{item}\n"
        if out_file:
            out_file.write(line)

        if os.path.isdir(path):
            next_prefix = prefix + ("    " if is_last else "|   ")
            print_tree(path, next_prefix, out_file)


# Generate the complete Markdown document
with open("project_source.md", "w", encoding="utf-8") as f:
    f.write("# Project Structure\n```text\nAssets\n")
    print_tree("Assets", prefix="", out_file=f)
    f.write("```\n\n---\n\n")

    # Recursively find and append all C# files
    for root, dirs, files in os.walk("Assets"):
        # Ignore Unity internal directories
        dirs[:] = [
            d
            for d in dirs
            if d
            not in ["Library", "Temp", "Logs", "obj", "Build", "Builds", "UserSettings"]
        ]
        for file in sorted(files):
            if file.endswith(".cs"):
                file_path = os.path.join(root, file)
                f.write(f"# File: {file_path}\n```csharp\n")
                try:
                    # Read C# file using strict UTF-8
                    with open(
                        file_path, "r", encoding="utf-8", errors="replace"
                    ) as code_f:
                        f.write(code_f.read())
                except Exception as e:
                    f.write(f"// Error reading file: {e}\n")
                f.write("\n```\n\n")

print("Success! Generated project_source.md with full Russian support.")
