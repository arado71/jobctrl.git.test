Dim content, objFSO, objFile
Const ForReading = 1
Set objFSO = CreateObject("Scripting.FileSystemObject")
Set objFile = objFSO.OpenTextFile(WScript.Arguments.Item(0), ForReading)
content = objFile.ReadAll
objFile.Close

Dim re, matches
Set re = new regexp
re.Pattern = "RETURN (\d+)"
re.IgnoreCase = true
re.MultiLine = True

Set matches = re.Execute(content)
If matches.Count > 0 Then
  Set match = matches(0)
  'msg = "Found match """ & match.Value & """ at position " & match.FirstIndex & vbCRLF
  If match.SubMatches.Count > 0 Then
    For I = 0 To match.SubMatches.Count-1
      Wscript.Stdout.Write match.SubMatches(I)
      WScript.Quit 0
      'msg = msg & "Group #" & I+1 & " matched """ & match.SubMatches(I) & """" & vbCRLF
    Next
  End If
  'msgbox msg, 0, "VBScript Regular Expression Tester"
Else
  'msgbox "No match", 0, "VBScript Regular Expression Tester"
End If

WScript.Quit 1