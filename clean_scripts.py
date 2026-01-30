import os
import re
import glob

root_dir = "E:/Develop/Unity Editor/Project one/Assets/_DungeonDefence/Scripts"

def clean_file(filepath):
    try:
        with open(filepath, 'r', encoding='utf-8') as f:
            text = f.read()
        
        def replacer(match):
            s = match.group(0)
            if s.startswith('/'):
                return ""
            return s
            
        # Pattern to match comments or strings
        # We match strings to ensure we don't remove // inside strings
        pattern = r'//.*?$|/\*.*?\*/|\'(?:\\.|[^\\\'])*\'|"(?:\\.|[^\\"])*"'
        
        text = re.sub(pattern, replacer, text, flags=re.DOTALL | re.MULTILINE)
        
        # Remove #region and #endregion
        text = re.sub(r'^\s*#(region|endregion).*?$', '', text, flags=re.MULTILINE)
        
        # Reduce multiple newlines
        text = re.sub(r'\n\s*\n\s*\n+', '\n\n', text)
        
        with open(filepath, 'w', encoding='utf-8') as f:
            f.write(text)
        print("Cleaned: " + filepath)
            
    except Exception as e:
        print("Error processing " + filepath + ": " + str(e))

files = glob.glob(os.path.join(root_dir, '**', '*.cs'), recursive=True)
print("Found " + str(len(files)) + " files.")
for f in files:
    clean_file(f)