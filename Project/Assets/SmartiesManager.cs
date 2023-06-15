
using System;
using System.Net;
using System.Collections.Generic;

using UnityEngine;
using System.Collections;

public class SmartiesManager :  MonoBehaviour {

	Setup setup;
	InputHandler input_handler;
	Wall wall;

	Smarties smarties;
	bool started = false;
	bool showExpeWidgets = false;

	SmartiesWidget _attach_widget;

	List<SmartiesWidget> expeWidgets;
	List<SmartiesWidget> interfaceWidgets;

	void Start() {
		Debug.Log("Start SmartiesManager");
		setup = GameObject.Find("ScriptManager").GetComponent<Setup>();

		if (!setup.smarties){
			smarties = null;
			return;
		}
		wall = setup.wall;

		// see the Smarties javadoc
		smarties = new Smarties(
			(int)wall.Width(), (int)wall.Height(), wall.ColumnsAmount(), wall.RowsAmount()
		);
	
		/*
		//to add a widget : 
			//first set up the grid
		smarties.initWidgets(4,3);
			//then initialize the container variable
		SmartiesWidget wid;
			//then add the wanted widget
		wid = smarties.addWidget(SmartiesWidget.SMARTIES_WIDGET_TYPE_*, "text', x_pos, y_pos, x_size, y_size);
			//then some stuff I must check for utilities
		wid.handler = letterHHandler; //might modify
		wid_list.Add(wid); //optional ?
		*/

		
		// banzai
		smarties.SmartiesUpdate += OnSmartiesUpdate;
		smarties.Run();

		input_handler = GameObject.Find("Operator(Clone)").GetComponent<InputHandler>();
		input_handler.RegisterDevice("Smarties", this);

		Debug.Log ("Smarties started");
		started = true;
	}

	void OnSmartiesUpdate(Smarties s, SmartiesEvent e) {
		switch (e.type) {
			case SmartiesEvent.SMARTIES_EVENTS_TYPE_CREATE:
				Debug.Log("SM Create Puck: " + e.id);
				input_handler.CreateMCursor(this, e.id, e.x, e.y, SmartiesColors.getPuckColorById(e.id));
				break;

			case SmartiesEvent.SMARTIES_EVENTS_TYPE_DELETE:
				Debug.Log("SM Delete Puck: " + e.id);
				input_handler.RemoveMCursor(this, e.id);
				smarties.deletePuck(e.id);
				break;

			case SmartiesEvent.SMARTIES_EVENTS_TYPE_START_MOVE:
				input_handler.StartMoveMCursor(this, e.p.id, e.p.x, e.p.y,
					(e.mode == SmartiesEvent.SMARTIES_GESTUREMOD_DRAG) );
				break;

			case SmartiesEvent.SMARTIES_EVENTS_TYPE_MOVE:
				input_handler.MoveMCursor(this, e.p.id, e.p.x, e.p.y);
				break;

			case SmartiesEvent.SMARTIES_EVENTS_TYPE_END_MOVE:
				input_handler.StopMoveMCursor(this, e.p.id, e.p.x, e.p.y);
				break;

			case SmartiesEvent.SMARTIES_EVENTS_TYPE_WIDGET:
				if (e.widget.handler != null) {
					e.widget.handler(e.widget, e, this);
				}
				break;

			default:
				break;
		}
	}
}