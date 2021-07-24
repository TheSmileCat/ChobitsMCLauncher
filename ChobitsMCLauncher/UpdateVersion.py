import os, sys, re

path = sys.argv[0][0:sys.argv[0].rindex("\\")]
file = open(path + "\\Version.cs", "r", encoding="utf-8")
strs = file.readlines()
i = 0
while i < len(strs):
    if (re.match("^\s+public\sstatic\sreadonly\sint\sversion\s=\s[0-9]+;",
                 strs[i])):
        vis = strs[i][strs[i].index("= ") + 2:len(strs[i]) - 2]
        vis = int(vis) + 1
        strs[i] = "        public static readonly int version = " + str(vis) + ";\n"
        print("版本已更新至" + str(vis))
        break
    i += 1
file.close()
file = open(path + "\\Version.cs", "w+", encoding="utf-8")
file.writelines(strs)
file.close()
file1 = open(path + "\\.pubversion", "w+", encoding="utf-8")
file1.write(str(vis))
file1.close()