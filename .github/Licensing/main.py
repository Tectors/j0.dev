import os

root_dir = r"./Licensing"
output_file = os.path.join(root_dir, "NOTICE")

license_filenames = {
    "LICENSE", "LICENSE.txt", "LICENSE.md",
    "license", "license.txt", "license.md",
    "Licence", "licence.txt", "licence.md",
    "THIRD_PARTY_NOTICES.md", "GPLv3.txt",
    "NOTICE"
}

with open(output_file, "w", encoding="utf-8") as outfile:
    for foldername, _, filenames in os.walk(root_dir):
        for filename in filenames:
            file_path = os.path.join(foldername, filename)

            if file_path == r"./Licensing\NOTICE":
                continue

            if filename in license_filenames:
                relative_path = os.path.relpath(file_path, root_dir)

                outfile.write("\n" + "=" * 40 + "\n")
                outfile.write(f"File: {relative_path}\n")
                outfile.write("=" * 40 + "\n\n")

                with open(file_path, "r", encoding="utf-8") as infile:
                    outfile.write(infile.read())
                    outfile.write("\n")
