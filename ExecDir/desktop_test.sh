#!/bin/bash
EXEC="linux_start.x86_64"
WALL="DESKTOP"
PART_AMOUNT=4
MASTER_ID="m"
PART_ID="p"
MASTER_ONLY=0
MASTER_EDITOR=0
FS=0

if [ $MASTER_EDITOR == 0 ]; then
	echo "executing for operator"
	echo "./$EXEC -popupwindow -screen-fullscreen $FS -screen-width 1024 -screen-height 512 -wall $WALL -sw 1024 -sh 512 -r $MASTER_ID -pa $PART_AMOUNT -mo $MASTER_ONLY &"

	./$EXEC -popupwindow -screen-fullscreen $FS -screen-width 1024 -screen-height 512 -wall $WALL -sw 1024 -sh 512 -r $MASTER_ID -pa $PART_AMOUNT -mo $MASTER_ONLY -logfile logs/log_master.txt &

sleep 8
fi

if [ $MASTER_ONLY == 0 ]; then
	echo "executing for participants"
	./$EXEC -popupwindow -screen-fullscreen $FS -screen-width 512 -screen-height 256 -wall $WALL -sw 512 -sh 256 -r $PART_ID -x 0 -y 0 -logfile logs/log_p1.txt &
	./$EXEC -popupwindow -screen-fullscreen $FS -screen-width 512 -screen-height 256 -wall $WALL -sw 512 -sh 256 -r $PART_ID -x 512 -y 0 -logfile logs/log_p2.txt &
	./$EXEC -popupwindow -screen-fullscreen $FS -screen-width 512 -screen-height 256 -wall $WALL -sw 512 -sh 256 -r $PART_ID -x 0 -y 256 -logfile logs/log_p3.txt &
	./$EXEC -popupwindow -screen-fullscreen $FS -screen-width 512 -screen-height 256 -wall $WALL -sw 512 -sh 256 -r $PART_ID -x 512 -y 256 -logfile logs/log_p4.txt &
fi
