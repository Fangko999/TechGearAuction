import os

controllers_dir = r"d:\Code\TechGearAuction\TechGearAuction.API\Controllers"

for filename in os.listdir(controllers_dir):
    if filename.endswith(".cs"):
        filepath = os.path.join(controllers_dir, filename)
        with open(filepath, 'r', encoding='utf-8') as f:
            content = f.read()

        lines = content.split('\n')
        new_lines = []
        i = 0
        while i < len(lines):
            line = lines[i]
            stripped = line.strip()
            
            if stripped == 'try':
                if i + 1 < len(lines) and lines[i+1].strip() == '{':
                    i += 2
                    continue
            
            if stripped.startswith('catch') and '(' in stripped and ')' in stripped:
                if '{' in stripped and '}' in stripped:
                    i += 1
                    continue
                elif i + 1 < len(lines) and lines[i+1].strip() == '{':
                    i += 2
                    while i < len(lines) and lines[i].strip() != '}':
                        i += 1
                    i += 1
                    continue
            
            if stripped == '}':
                j = i + 1
                is_try_end = False
                while j < len(lines):
                    peek = lines[j].strip()
                    if peek == '':
                        j += 1
                        continue
                    if peek.startswith('catch'):
                        is_try_end = True
                    break
                if is_try_end:
                    i += 1
                    continue
            
            new_lines.append(line)
            i += 1
            
        new_content = '\n'.join(new_lines)
        if new_content != content:
            with open(filepath, 'w', encoding='utf-8') as f:
                f.write(new_content)
            print(f"Refactored {filename}")

