using UnityEngine;

public static class CursorsTex {
    public static Texture2D SimpleCursor(Color main, Color black, int hw, int ct, int cl){
    	int cw = 2 * hw;
    	Texture2D cursor = new Texture2D(cw, cw);
    	Color trans = new Color(1,1,1,0);
    	for (int y = 0; y < cursor.height; y++){
            for (int x = 0; x < cursor.width; x++){
            	Color color = trans;
            	if (y <= hw+ct+cl-1 && y >= hw-(ct+cl+1)){
            		if (x < cl || x > cw -cl-1){
            			color = black;
            		} else if ((y > hw+ct-1 || y < hw-(ct+1)) &&
            			!(x <= hw+ct-1 && x >= hw-(ct+1))) {
            			color = black;
            		} else{
            			color = main;
            		}
            	}
            	else if (x <= hw+ct+cl-1 && x >= hw-(ct+cl+1)){
            		if (y < cl || y > cw -cl-1){
            			color = black;
            		} else if (x > hw+ct-1 || x < hw-(ct+1)) {
            			color = black;
            		} else{
            			color = main;
            		}
            	}
                cursor.SetPixel(x, y, color);
            }
        }
        cursor.Apply();
    
        return cursor;
    }
}
