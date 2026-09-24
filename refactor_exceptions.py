import os
import re

app_dir = r"d:\Code\TechGearAuction\TechGearAuction.Application\Features"

for root, _, files in os.walk(app_dir):
    for filename in files:
        if filename.endswith(".cs"):
            filepath = os.path.join(root, filename)
            with open(filepath, 'r', encoding='utf-8') as f:
                content = f.read()

            if "throw new Exception" in content:
                # Add using TechGearAuction.Domain.Exceptions if not present
                if "using TechGearAuction.Domain.Exceptions;" not in content:
                    # Put it after the first using or at the top
                    content = "using TechGearAuction.Domain.Exceptions;\n" + content

                # Replace standard "throw new Exception" with domain exceptions based on heuristics
                # 1. "not found" -> NotFoundException
                # 2. "permission", "allowed", "banned" -> ForbiddenException or BannedUserException
                # 3. Otherwise -> BusinessRuleException
                
                lines = content.split('\n')
                new_lines = []
                for line in lines:
                    if "throw new Exception" in line:
                        # Extract the string inside Exception(...)
                        match = re.search(r'throw new Exception\((.*?)\);', line)
                        if match:
                            msg = match.group(1).lower()
                            new_ex = "BusinessRuleException"
                            if "not found" in msg:
                                # We can't easily parse entity name, just throw BusinessRuleException or generic NotFoundException
                                new_ex = "NotFoundException"
                                line = line.replace(f"Exception({match.group(1)})", f"NotFoundException(\"Entity\", {match.group(1)})")
                            elif "permission" in msg or "allow" in msg or "authoriz" in msg:
                                new_ex = "ForbiddenException"
                                line = line.replace(f"Exception({match.group(1)})", f"ForbiddenException({match.group(1)})")
                            elif "banned" in msg:
                                new_ex = "BannedUserException"
                                line = line.replace(f"Exception({match.group(1)})", f"BannedUserException({match.group(1)}, 0)")
                            else:
                                line = line.replace(f"Exception({match.group(1)})", f"BusinessRuleException({match.group(1)})")
                    new_lines.append(line)
                
                new_content = '\n'.join(new_lines)
                with open(filepath, 'w', encoding='utf-8') as f:
                    f.write(new_content)
                print(f"Updated exceptions in {filename}")

