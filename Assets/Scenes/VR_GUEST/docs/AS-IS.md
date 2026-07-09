This is a stand-alone VR application made with Unity 6000.4.5.f1.
## Phase 1: VR_GUEST_Home loading
The user starts the application, the initial scene, called VR_GUEST_Home is loaded. To do so, the application uses an internal database automatically updated with the information from a server. The idea is to retrieve two information, the gates recorded in the database and their position. Once known, there's a method that places the gates within a map as interactable objects.

Problems $\to$ What I would like:
1. **Server**:
    The server is no longer available and it has been substituted with a new one that provides me only one information, whether the gate is active or not.
    $\to$ I prefer a solution where the gates are statically saved in the application (also because I can update this information whenever a new scene is created), and the only thing is updated is the status (just for reference since it is a variable I'm not going to use since in VR I can show the gates whether it is actually active or not. 
2. **Network Protocol**:
	As it is, it is using http request to communicate with the old server $\to$ with the new one I must use WebSocket / TCP (Unity native). 



## Phase 2: Guest interaction
Once *phase 1* is completed, the gates placed within the map act as an interactable object to access the VR scene coupled with that gate. As it is, the interaction is limited by two factor, the presence of the gate's information in the server's database (the one dismissed) and the presence of the coupled scene in the build of the application.
So, when the object is instantiated, the following logic is executed:
1. The object tracks the scenes that are put in the build of the application.
2. The interaction user-object triggers a logic that checks if the object name is within the list of scenes in the build of the application.

Problems  $\to$ What I would like:
1.  
	Without the server information no gate is going to allow the user the access to the VR scene, even if it is implemented.
2. 
	The interaction user-gate lacks of telemetry data $\to$ A new functionality must be added, when the user interacts with a gate, if he accesses the coupled VR scene, I must send a payload to the new server with these information:
		```json
		{
		  "id_visore": "MQ3:CAGLIARI:A1", %%for instance%%
		  "ingresso_vr": true,
		  "uscita_vr": false,
		  "datetimestamp_vr": "2026-05-22T12:25:00Z"
		}
		```
3. 
	Ciao
4. 
	Ciao 

## Phase 3: VR scene
The user successfully gets in the VR scene and he is exploring it. The following implementation is entirely to be done. 
Three scenarios may happen:
1. The user gets out of the scene. $\to$ go back to the initial scene and send the telemetry data to the new server:
	   ```json
	   {
		  "id_visore": "MQ3:CAGLIARI:A1", %%for instance%%
		  "ingresso_vr": false,
		  "uscita_vr": true,
		  "datetimestamp_vr": "2026-05-22T12:30:00Z"
		}```
2. the user takes off the headset. $\to$ background task, if the headset is not put back on after a certain amount of time, send the telemetry data and go back to the initial scene.
   	   ```json
	   {
		  "id_visore": "MQ3:CAGLIARI:A1", %%for instance%%
		  "ingresso_vr": false,
		  "uscita_vr": true,
		  "datetimestamp_vr": "2026-05-22T12:30:00Z"
		}
3. the user turn off the headset. $\to$ if it is possible I want to send the telemetry data before turning it off.
   	   ```json
	   {
		  "id_visore": "MQ3:CAGLIARI:A1", %%for instance%%
		  "ingresso_vr": false,
		  "uscita_vr": true,
		  "datetimestamp_vr": "2026-05-22T12:30:00Z"
		}
