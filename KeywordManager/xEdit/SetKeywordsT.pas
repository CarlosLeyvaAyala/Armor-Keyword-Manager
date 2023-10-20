unit SIM_SetKeywords;
{
    Hotkey: Ctrl+Shift+K
    
    Auto-generated script for applying keywords from the app.

    Script version: 1.0.0
}
interface

uses xEditApi;

implementation

function FileByName(s: string): IInterface;
    var
    i: integer;
    begin
    Result := nil;

    for i := 0 to FileCount - 1 do 
        if GetFileName(FileByIndex(i)) = s then begin
            Result := FileByIndex(i);
            Exit;
        end;
end;

function KeywordIndex(e: IInterface; edid: string): Integer;
var
    kwda: IInterface;
    n: integer;
begin
    Result := -1;
    kwda := ElementByPath(e, 'KWDA');
    for n := 0 to ElementCount(kwda) - 1 do
        if GetElementEditValues(LinksTo(ElementByIndex(kwda, n)), 'EDID') = edid then begin
            Result := n;
            Exit;
        end;
end;

procedure AddKeyword(e: IInterface; edid, fileName: string);
var
    keys: IwbGroupRecord;
    key, k, esp, f: IInterface;
    i: Integer;
begin
    if KeywordIndex(e, edid) <> -1 then Exit; // Keyword already added

    esp := FileByName(fileName);

    if not Assigned(esp) then begin 
        AddMessage(Format('Can not add keyword "%s" because "%s" does not exist.', [edid, fileName]));
        Exit;
    end;

    AddMasterIfMissing(GetFile(e), GetFileName(esp));

    key := MainRecordByEditorID(GroupBySignature(esp, 'KYWD'), edid);
    if Assigned(key) then begin
        // Add new keyword
        k := ElementAssign(ElementByPath(e, 'KWDA'), HighInteger, nil, false);
        SetEditValue(k, Name(key));
    end
    else 
        AddMessage(Format('Keyword "%s" was not found in "%s".', [edid, fileName]));
end;

function Initialize: Integer;
var 
    item: IInterface;
    <declarations>: IInterface;
begin
<initPlugins>

<addKeywords>
end;

end.
