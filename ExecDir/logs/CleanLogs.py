def clean(file_path):
    with open(file_path, 'r') as file:
        lines = file.readlines()
    # Remove the first 37 lines
    lines = lines[37:]
    
    # Removes this lines that are starting with "(Filename" 
    lines = [line for line in lines if not line.startswith('(Filename')]
    # Remove lines that follow a paragraph's fst line
    paragraphs = []
    paragraph_strt = False
    for line in lines:
            line = line.strip()
            if line:
                if not paragraph_strt:
                    paragraphs.append(line)
                    paragraph_strt = True
            else:
                paragraph_strt = False
    output = '\n'.join(paragraphs)

    with open(file_path, 'w') as file:
        file.write(output)

file_path = "log_master_alones.txt"
clean(file_path)
