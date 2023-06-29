#!/bin/bash
EXEC="linux_start.x86_64"
WALL="DESKTOP"
PART_AMOUNT=1
MASTER_ID="m"
PART_ID="p"
MASTER_ONLY=1
FS=0

echo "executing for operator"
echo "./$EXEC -popupwindow -screen-fullscreen $FS -screen-width 1440 -screen-height 470 -wall $WALL -sw 1440 -sh 470 -r $MASTER_ID -pa $PART_AMOUNT -mo $MASTER_ONLY &"

./$EXEC -popupwindow -screen-fullscreen $FS -screen-width 1440 -screen-height 470 -wall $WALL -sw 1440 -sh 470 -r $MASTER_ID -pa $PART_AMOUNT -mo $MASTER_ONLY -logfile logs/log_master_alones.txt &
