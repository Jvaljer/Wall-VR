using System;
using UnityEngine;
using System.Collections;

public class Wilder : Wall {

	struct Screen {	
		public Vector3  origin;
		public Vector3  vX;
		public Vector3  vY;
		public Vector3  vZ;
	}

	static Screen screen = new Screen {
		origin = new Vector3(2974.2597f, 8.09393995f, 2215.8808f),
		vX = new Vector3(-0.4099097f, -0.0013532f,  0.0039963f),
		vY = new Vector3( 0.0034272f, -0.0015333f, -0.4099142f),
		vZ = new Vector3(-0.0005608f,  0.1680141f, -0.0006332f),
	};


	public Vector3 WallCoordinate(Vector3 pos, Vector3 dir) {
		Vector3 position = new Vector3(pos[0], pos[1], pos[2]);
		Vector3 direction = new Vector3(dir[0], dir[1], dir[2]);

		Vector3 l;
		l = position - screen.origin;
		
		Vector3 ox2 = Vector3.Cross(screen.vY, direction);
		Vector3 oy2 = Vector3.Cross(direction, screen.vX);
		Vector3 oz2 = Vector3.Cross(screen.vX, screen.vY);

		float d1  = (screen.vX.x * ox2.x)+  (screen.vX.y * ox2.y) + (screen.vX.z * ox2.z);
		float d2 =  (screen.vY.x * oy2.x) + (screen.vY.y * oy2.y) + (screen.vY.z * oy2.z);
		float d3 = (direction.x * oz2.x) + (direction.y * oz2.y) + (direction.z * oz2.z);

		Vector3 p;
		p.x = ((l.x * ox2.x) + (l.y * ox2.y) + (l.z * ox2.z))/d1;
		p.y = ((l.x * oy2.x) + (l.y * oy2.y) + (l.z * oy2.z))/d2;
		p.z = ((l.x * oz2.x) + (l.y * oz2.y) + (l.z * oz2.z))/d3;

		
		float sign = -1;
		if (dir[1] < 0) {
			sign = 1;
		}

		return new Vector3(p.x, p.y, sign);
	}

    //getters
	public float Width(){
		return 14400; //5920 mm 
	}

	public float Height(){
		return 4800; //1975 mm
	}

	public float SingleScreenWidth(){
		return 960;
	}

	public float SingleScreenHeight(){
        return 960;
	}

	public float BezelWidth(){
		return 0;
	}

	public float BezelHeight(){
		return 0;
	}

	public int ColumnsAmount(){
		return 15;
	}

	public int RowsAmount(){
		return 5;
	}

	public float PixelSizeMM() { 
        return 0.411f; 
    }
	public float BottomHeight(){ 
        return 240.8f; 
    }

	public string ViconHost(){
		return "192.168.2.3"; 
	}

}
