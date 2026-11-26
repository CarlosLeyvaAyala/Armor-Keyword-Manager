:: This batch file is used to copy all files from your preferred texture set
:: to one defined by some careless mod author who forces you to use the same
:: textures (s)he did (examples: Bijin, Ordinary Women, Vilja, Sofia...).
:: Use this to fix the dreaded face/body/hands textures mismatch, where the body
:: (which always uses the textures you actually like) doesn't match the head,
:: hand and/or feet.

: Usage:
:: 1) Put this file inside the folder where THE TEXTURES YOU ACTUALLY WANT ARE.
:: 2) Drag and drop the folder you want to be replaced to this file. That's all.

:: This batch script will automatically replace all files in all subfolders.
:: That's quite useful if the author made one subfolder for each NPC, for example
:: Bijin, which has maaaaany subfolders. You can just drag and drop the folder
:: called "character" to this file and it will replace all textures in all subfolders.

@echo off
Title Copying textures from source to destination

set sp=%~dp0%
echo Copying files FROM:
echo %sp%
echo -------------------------------------------------

:: Recurse all subdirs

for /r %%a in (.) do (
    :: enter the directory
    pushd %%a
    cd
    echo -------------------------------------------------
    echo Replacing files IN
    echo %%a
    xcopy /s /y /u /q "%sp%" "%%~dpfa"
    :: leave the directory
    popd
)
pause
