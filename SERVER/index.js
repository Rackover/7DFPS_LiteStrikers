const crypto = require('crypto');
const express = require('express');
const { createServer } = require('http');
const WebSocket = require('ws');

const app = express();

const server = createServer(app);
const wss = new WebSocket.Server({ server });

const port = 1235;
const backyardSize = 100;
const dropAckTimeout = 1;
const spawnDistance = 10;

const LOADOUTS =
{
	LMG:1,
	TRIPLE:2,
	HOMING:3
}

const PROTOCOL_MOVESTATE = "MOV";
const PROTOCOL_UPDATE_SCORE = "SCO";
const PROTOCOL_STATE = "STT";
const PROTOCOL_ACKNOWLEDGE_STATE = "AKS";
const PROTOCOL_DISCONNECT_PLAYER = "DIS";
const PROTOCOL_SHOOTSTATE = "SHT";
const PROTOCOL_SET_LOADOUT = "LDT";
const PROTOCOL_MEOW = "MEW";
const PROTOCOL_MISSILE_BIRTH = "MBI";
const PROTOCOL_MISSILE_MOVESTATE = "MMV";
const PROTOCOL_ELIMINATE_PLAYER = "ELP";
const PROTOCOL_SPAWN_PLAYER = "SPP";
const PROTOCOL_REQUEST_SPAWN = "RSP";

const handlers = {};
handlers[PROTOCOL_MOVESTATE] = send_position_to_everyone_but_me;
handlers[PROTOCOL_ACKNOWLEDGE_STATE]=  acknowledge_client;
handlers[PROTOCOL_MEOW]= send_meow_to_everyone;
handlers[PROTOCOL_SHOOTSTATE]= send_shoot_to_everyone;
handlers[PROTOCOL_SET_LOADOUT]= switch_my_loadout;
handlers[PROTOCOL_REQUEST_SPAWN]= request_spawn;

const forbiddenProtocols = [
	PROTOCOL_UPDATE_SCORE, 
	PROTOCOL_STATE, 
	PROTOCOL_DISCONNECT_PLAYER,
	PROTOCOL_MISSILE_BIRTH,
	PROTOCOL_ELIMINATE_PLAYER,
	PROTOCOL_SPAWN_PLAYER,
	PROTOCOL_MISSILE_MOVESTATE
];

for(i in forbiddenProtocols){
	const protocol = forbiddenProtocols[i];
	handlers[protocol]= function(me, ws, data){ log("someone ("+me.id+") sent me a "+protocol+" ??");};
}

let clients = {};
let idCounter = 0;
let missileIdCounter = 0;
let clientsAwaitingAck = [];
let clientsScores = {};

const missileUpdateFrequencyMs = 10;
const homingMissileSpeed = 15; // meters per second

wss.on('connection', function(ws) {
    
    const id = ++idCounter;
    
    clients[id] = make_client(id, ws);
    clientsAwaitingAck.push(id);
    
    log("HELLO Client "+id+" joined. Spawned them at position "+JSON.stringify(clients[id].position));

    // Test
    // let interval = setInterval(function(){
        // ws.send(PROTOCOL_MOVESTATE+JSON.stringify({
            // id: 230,
            // position: {x: 4, y:0.5, z:5 /*  Math.sin((new Date()).getMilliseconds()/10)*400 */},
            // rotation: "0 0 0 0"
        // }));
    // }, 100);
    
    ws.on('message', function(data) {
        // Has to be a string, fuck it
        let controller = data.substr(0, 3);
        let contents = data.substr(3, data.length);
        
        if (handlers.hasOwnProperty(controller)){
            handlers[controller](clients[id], ws, contents);
        }
        else{
            log("! Unknown junk controller ["+controller+"] with data "+data+" from client "+id);
        }
    });

    ws.on('close', function() {
        send_disconnect_player(id);   
        log("GOODBYE Client "+id+".");
        // clearInterval(interval);
        delete clients[id];
    });
    
    // Send state
    clients[id].isYou = true;
    send_state_to(ws);
    clients[id].isYou = false;
    
    // Acknowledge anyway after a while to avoid freezing the game
    setTimeout(function(){acknowledge_client({id:id}, null, null)}, dropAckTimeout);
    
});

server.listen(port, function() {
    log('Listening on port '+port);
});

setInterval(function(){
    log(Object.values(clients).length+" players currently active on the server");
}, 30000);

function send_state_to(ws){
    ws.send(PROTOCOL_STATE+get_serialized_state());
}

function get_serialized_state(){
    return JSON.stringify({
        clients: Object.values(clients).map(function (client) {
            return {
                id: client.id,
				isSpawned: client.isSpawned,
                position: client.position,
                rotation: client.rotation,
                isYou: client.isYou,
				loadout: client.loadout,
				color: client.color
            };
        }),
        scores: clientsScores,
		map: "default"
    });
}

function get_spawnpoint(){
    let randomPoint = {x: backyardSize/2, z:backyardSize/2};
    let isOk = false;
    
    while(!isOk){
        isOk = get_nearest_distance_with_any_player(randomPoint) > spawnDistance;
        
        if (!isOk){
            randomPoint = {
                x: (Math.random()*2 - 1) * backyardSize,
                z: (Math.random()*2 - 1) * backyardSize,
            }
        }
    }
    
    return randomPoint;
}

function get_nearest_distance_with_any_player(vec){
    let distance = Infinity;
    for(id in clients){
        let newDist = get_distance(clients[id].position, vec);
        if (newDist < distance){
            distance = newDist;
        }
    }
    
    return distance;
}

function get_distance(vec1, vec2){
    const a = vec1.x - vec2.x;
    const b = vec1.z - vec2.z;

    return Math.sqrt( a*a + b*b );
}

function make_client(id, ws){
	console.log(Math.floor(Math.random()*Object.keys(LOADOUTS).length));
    return {
        id:id,
        socket:ws,
		isSpawned: false,
        position: get_spawnpoint(),
        rotation: [0, 0, 0, 0],
        loadout: Math.floor(Math.random()*Object.keys(LOADOUTS).length),
		color: 0
    };
}

function log(obj){
    const str = (new Date()).toISOString().slice(0, 19).replace(/-/g, "/").replace("T", " ");
    console.log(str+" "+obj);
}


/////////////////////////////////
//
// PROTOCOL

function send_position_to_everyone_but_me(me, ws, data){
    for(clientId in clients){
        if (clientId == me.id){
            let parsed = JSON.parse(data);
            clients[clientId].position = parsed.position;
            clients[clientId].isSneaking = parsed.isSneaking;
            continue;            
        }
        
        clients[clientId].socket.send(PROTOCOL_MOVESTATE+data);
    }
}

function send_caught_to_everyone_but_me_and_increase_score(me, ws, data){
    try{
        log("Client "+me.id+" caught bird "+data);
        
        if (!clientsScores[me.id]){
            clientsScores[me.id] = 0;
        }
        
        clientsScores[me.id]++;
        
        for(clientId in clients){            
            clients[clientId].socket.send(PROTOCOL_UPDATE_SCORE+JSON.stringify({
                clientId:me.id,
                newScore: clientsScores[me.id]
            }));
        }
    }
    catch(e){
        log(e);
    }
    
}

function send_meow_to_everyone(me, ws, data){
    for(clientId in clients)
	{            
        clients[clientId].socket.send(PROTOCOL_MEOW+me.id);
    }
}

// shootstate
/*
{
	target: int,
	position: {xyz},
	initialRotation: float[4]
}
*/
function send_shoot_to_everyone(me, ws, data)
{
	if (me.loadout == LOADOUTS.LMG)
	{
		for(clientId in clients)
		{            
			clients[clientId].socket.send(PROTOCOL_SHOOTSTATE+me.id);
		}
	}
	else
	{
		// missile birth
		
		const isHoming = me.loadout == LOADOUTS.HOMING;
		
		const missile = {
			owner: me.id,
			type: me.loadout,
			target: data.target,
			position: data.position,
			initialRotation: data.rotation,
			id: isHoming ? ++missileIdCounter : 0, // Only used for homing
			lifetime: me.loadout == LOADOUTS.TRIPLE ? 1.5 : 3 // seconds
		}
		
		for(clientId in clients)
		{            
			clients[clientId].socket.send(PROTOCOL_MISSILE_BIRTH+JSON.stringify(missile));
		}
		
		if (isHoming){
			// Missile update
			let interval = setInterval(function(){
				missile.lifetime -= missileUpdateFrequencyMs;
				if (missile.lifetime <= 0 || clients[missile.target] == undefined)
				{
					clearInterval(interval);
				}
				else
				{
					const target = clients[missile.target];
					
					const distanceVector = { 
						x: target.position.x - missile.position.x,
						y: target.position.y - missile.position.y,
						z: target.position.z - missile.position.z
					};
					
					const magnitude = Math.sqrt(
						x: directionVector.x * directionVector.x +
						y: directionVector.y * directionVector.y +
						z: directionVector.z * directionVector.z
					);
					
					const direction = {
						x: distanceVector.x / magnitude,
						y: distanceVector.y / magnitude,
						z: distanceVector.z / magnitude
					};
					
					// No inaccuracy
					missile.position = {
						x: missile.position.x + distanceVector.x * homingMissileSpeed * missileUpdateFrequencyMs,
						y: missile.position.y + distanceVector.y * homingMissileSpeed * missileUpdateFrequencyMs,
						z: missile.position.z + distanceVector.z * homingMissileSpeed * missileUpdateFrequencyMs
					}
					
					for(clientId in clients)
					{            
						clients[clientId].socket.send(PROTOCOL_MISSILE_MOVESTATE+JSON.stringify(missile));
					}
				}
				
			}, missileUpdateFrequencyMs);
				
		}
	}
	
}

function switch_my_loadout(me, ws, data)
{
	const loadout = data.loadout;
	
}

function send_disconnect_player(id){
    for(clientId in clients){ 
        clients[clientId].socket.send(PROTOCOL_DISCONNECT_PLAYER+id);
    }
}

function acknowledge_client(me, ws, data){
    const index = clientsAwaitingAck.indexOf(me.id);
    if (index > -1) {
        clientsAwaitingAck.splice(index, 1);
    }
}

