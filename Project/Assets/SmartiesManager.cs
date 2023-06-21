
using System;
using System.Net;
using System.Collections.Generic;

using UnityEngine;
using System.Collections;

public class SmartiesManager :  MonoBehaviour {

	Operator ope;
	Setup setup;
	InputHandler input_handler;
	Wall wall;

	Smarties smarties;

	SmartiesWidget _attach_widget;

	List<SmartiesWidget> expeWidgets;
	List<SmartiesWidget> interfaceWidgets;

	public void StartFromOpe(Setup S_, Operator O_){
		setup.logger.Msg("Smarties Manager starts", "C");
		ope = O_;
		input_handler = ope.gameObject.GetComponent<InputHandler>();
		setup = S_;

		if (!setup.smarties){
			setup.logger.Msg("Smarties not enabled for this setup", "S");
			smarties = null;
			return;
		}
		wall = setup.wall;

		// see the Smarties javadoc
		smarties = new Smarties(
			(int)wall.Width(), (int)wall.Height(), wall.ColumnsAmount(), wall.RowsAmount()
		);
	
		//first set up the grid
		smarties.initWidgets(4,3);

		//to add a widget : 
		/*
			//first initialize the container variable
		SmartiesWidget wid;
			//then add the wanted widget
		wid = smarties.addWidget(SmartiesWidget.SMARTIES_WIDGET_TYPE_BUTTON, "text", x_pos, y_pos, width, height);
			//then some stuff I must check for utilities
		wid.handler = WidgetHandler( ... ); //behavior when clicked
		*/

		SmartiesWidget widget;
		widget = smarties.addWidget(SmartiesWidget.SMARTIES_WIDGET_TYPE_BUTTON, "test", 1,1, 2,1);
		widget.handler = TextHandler_Test;
		
		//now we can run the smarties program
		smarties.SmartiesUpdate += OnSmartiesUpdate;
		smarties.Run();

		input_handler.RegisterDevice("Smarties", this);

		setup.logger.Msg("Smarties Manager has well started", "V");
	}

	void OnSmartiesUpdate(Smarties s, SmartiesEvent e) {
		switch (e.type) {
			case SmartiesEvent.SMARTIES_EVENTS_TYPE_CREATE:
				setup.logger.Msg("Smarties Manager -> Create Puck "+e.id,"V");
				input_handler.CreateMCursor(this, e.id, e.x, e.y, SmartiesColors.getPuckColorById(e.id));
				break;

			case SmartiesEvent.SMARTIES_EVENTS_TYPE_DELETE:
				setup.logger.Msg("Smarties Manager -> Delete Puck "+e.id,"V");
				input_handler.RemoveMCursor(this, e.id);
				smarties.deletePuck(e.id);
				break;

			case SmartiesEvent.SMARTIES_EVENTS_TYPE_START_MOVE:
				Debug.Log("SM -> Start Move");
				setup.logger.Msg("Smarties Manager -> Start Move","C");
				input_handler.StartMoveMCursor(this, e.p.id, e.p.x, e.p.y,
					(e.mode == SmartiesEvent.SMARTIES_GESTUREMOD_DRAG) );
				break;

			case SmartiesEvent.SMARTIES_EVENTS_TYPE_MOVE:
				setup.logger.Msg("Smarties Manager -> Move","C");
				input_handler.MoveMCursor(this, e.p.id, e.p.x, e.p.y);
				break;

			case SmartiesEvent.SMARTIES_EVENTS_TYPE_END_MOVE:
				setup.logger.Msg("Smarties Manager -> End Move","C");
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

	public bool TextHandler_Test(SmartiesWidget w, SmartiesEvent e, object user_data){
		setup.logger.Msg("widget test text", "C");
		return true;
	}
}