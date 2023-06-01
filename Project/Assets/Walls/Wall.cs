using System;
using UnityEngine;
using System.Collections;

public interface Wall {
    //all following values are in pixels !!!
	float Width();
	float Height();

	float SingleScreenWidth();
	float SingleScreenHeight();

    float BezelHeight();
    float BezelWidth();

	int ColumnsAmount();
	int RowsAmount();
		
	float PixelSizeMM(); //except for this one which is in milimeters
	float BottomHeight(); //and this one

	string ViconHost();

	Vector3 WallCoordinate(Vector3 pos, Vector3 dir);
}
