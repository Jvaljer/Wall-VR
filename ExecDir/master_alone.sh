#!/bin/bash
EXEC="linux_start.x86_64"
WALL="DESKTOP"
PART_AMOUNT=1
MASTER_ID="m"
PART_ID="p"
MASTER_ONLY=0
FS=0

echo "executing for operator"
echo "./$EXEC -popupwindow -screen-fullscreen $FS -screen-width 1024 -screen-height 512 -wall $WALL -sw 1024 -sh 512 -r $MASTER_ID -pa $PART_AMOUNT -mo $MASTER_ONLY &"

./$EXEC -popupwindow -screen-fullscreen $FS -screen-width 1024 -screen-height 512 -wall $WALL -sw 1024 -sh 512 -r $MASTER_ID -pa $PART_AMOUNT -mo $MASTER_ONLY -logfile log_master_alones.txt &
