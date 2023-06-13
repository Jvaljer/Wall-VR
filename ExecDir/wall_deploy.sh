#!/bin/bash

PROGHOME="/media/ssd/Demos/Wall-VR"
EXEC="linux_start.x86_64"
LIB="UnityPlayer.so"
DEBUG="UnityPlayer_s.debug"
DATA="linux_start_Data"
BUNDELS="Bundels"
CLEAN="yes"
CPDATA="yes"
CPBUNDELS="no"
CLEANBUNDELS="no"

LOGIN="wild"

for i in "$@"
do
case $i in
    -nc)
	CLEAN="no"
	 ;;
	 -nd)
	CPDATA="no"
    CLEAN="no"
	 ;;
     -b)
    CPBUNDELS="yes"
     ;;
esac
#shift
done

function colIP {
  case "$1" in
          "a" ) return 0;;
          "b" ) return 1;;
  esac
}

function deployone {
    startIp=$1

    if [ $CLEAN == "yes" ]; then
            # clean 
            echo "ssh $LOGIN@192.168.2.$startIp ; rm -f $PROGHOME/$EXEC; rm -rf $PROGHOME/$DATA"
            ssh $LOGIN@192.168.2.$startIp -o UserKnownHostsFile=/dev/null -o StrictHostKeyChecking=no "rm -f $PROGHOME/$EXEC; rm -f $PROGHOME/$LIB; rm -rf $PROGHOME/$DATA"
        fi
        echo "ssh $LOGIN@192.168.2.$startIp ; mkdir -p $PROGHOME"
        ssh $LOGIN@192.168.2.$startIp  -o UserKnownHostsFile=/dev/null -o StrictHostKeyChecking=no "mkdir -p $PROGHOME"
        echo "scp $EXEC $LOGIN@192.168.2.$startIp:$PROGHOME"
        scp $EXEC $LOGIN@192.168.2.$startIp:$PROGHOME
        echo "scp $LIB $LOGIN@192.168.2.$startIp:$PROGHOME"
        scp $LIB $LOGIN@192.168.2.$startIp:$PROGHOME
        echo "scp -r $DATA $LOGIN@192.168.2.$startIp:$PROGHOME"
        scp -r $DATA $LOGIN@192.168.2.$startIp:$PROGHOME
        if [ $CLEANBUNDELS == "yes" ]; then
            echo "ssh $LOGIN@192.168.2.$startIp ; rm -rf $BUNDELS"
            ssh $LOGIN@192.168.2.$startIp -o UserKnownHostsFile=/dev/null -o StrictHostKeyChecking=no "rm -rf $BUNDELS"
        fi
        if [ $CPBUNDELS == "yes" ]; then
            echo "scp -r $BUNDELS $LOGIN@192.168.2.$startIp:$PROGHOME"
            scp -r $BUNDELS $LOGIN@192.168.2.$startIp:$PROGHOME
        fi
}

echo "CLEAN:" $CLEAN
echo "CPDATA:" $CPDATA
echo "CPBUNDELS:" $CPBUNDELS

for col in {a..b}
do
    for row in {1..5}
    do
        colIP $col
        startIp=`expr $? + 1`
        startIp=`expr $startIp \* 10`
        startIp=`expr $startIp + $row` 
    	echo " "
    	deployone $startIp
      done
done

#deployone 2
#scp wilder-start.sh $LOGIN@192.168.2.2:$PROGHOME
