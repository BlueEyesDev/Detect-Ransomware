# Description of Program.cs

The code in the Program.cs file creates two hidden files named "_.txt" and "0.txt" and initializes a hook to monitor them for any modifications. But why were these specific names and file extensions chosen?

The reason for using these particular names and extensions is that ransomware often leverages the functionality of programs, and one of these items will always be in the top list. This allows, at the moment the program executes its `foreach` or `for` loop, the first item to always be detected. Thus, any modification or deletion will be detected.

Furthermore, the program will close all launched programs after Program.cs and delete any startups created thereafter. However, a small disclaimer: the code was written hastily and may contain bugs.

# Description of delete.cs

The code in the delete.cs delete files _.txt and 0.txt
