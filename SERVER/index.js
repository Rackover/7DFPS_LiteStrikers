const crypto = require('crypto');
const express = require('express');
const { createServer } = require('http');
const WebSocket = require('ws');

const app = express();

const server = createServer(app);
const wss = new WebSocket.Server({ server });

const port = 1235;
const arenaSize = 100;
const dropAckTimeout = 1;
const spawnDistance = 10;

const LOADOUTS =
{
	LMG:1,
	TRIPLE:2,
	HOMING:3
}

const PROTOCOL_MOVESTATE = "MOV";
const PROTOCOL_UPDATE_PLAYER = "UPP";
const PROTOCOL_STATE = "STT";
const PROTOCOL_ACKNOWLEDGE_STATE = "AKS";
const PROTOCOL_DISCONNECT_PLAYER = "DIS";
const PROTOCOL_SHOOTSTATE = "SHT";
const PROTOCOL_SET_LOADOUT = "LDT";
const PROTOCOL_MEOW = "MEW";
const PROTOCOL_MISSILE_BIRTH = "MBI";
const PROTOCOL_MISSILE_MOVESTATE = "MMV";
const PROTOCOL_ELIMINATE_SELF= "ELS";
const PROTOCOL_SPAWN_PLAYER = "SPP"; // Identical to PROTOCOL_UPDATE_PLAYER
const PROTOCOL_UPDATE_SCORE = "SCO";
const PROTOCOL_REQUEST_SPAWN = "RSP";

const handlers = {};
handlers[PROTOCOL_MOVESTATE] = send_position_to_everyone_but_me;
handlers[PROTOCOL_ACKNOWLEDGE_STATE]=  acknowledge_client;
handlers[PROTOCOL_MEOW]= send_meow_to_everyone;
handlers[PROTOCOL_SHOOTSTATE]= send_shoot_to_everyone;
handlers[PROTOCOL_SET_LOADOUT]= switch_my_loadout;
handlers[PROTOCOL_REQUEST_SPAWN]= request_spawn;
handlers[PROTOCOL_ELIMINATE_SELF] = eliminate_myself;

const forbiddenProtocols = [
	PROTOCOL_UPDATE_SCORE, 
	PROTOCOL_STATE, 
	PROTOCOL_DISCONNECT_PLAYER,
	PROTOCOL_MISSILE_BIRTH,
	PROTOCOL_SPAWN_PLAYER,
	PROTOCOL_MISSILE_MOVESTATE,
	PROTOCOL_UPDATE_PLAYER
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

app.get('/', function(req, res){
	res.json(get_state());
});

wss.on('connection', function(ws) {
    
    const id = ++idCounter;
    
    clients[id] = make_client(id, ws);
    clientsAwaitingAck.push(id);
    
    log("HELLO Client "+id+" joined. Awaiting spawn request");

    ws.on('message', function(data) {
        // Has to be a string, fuck it
        const controller = data.substr(0, 3);
        const contents = data.substr(3, data.length);
        
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
		delete clientScores[id];
    });
    
    // Send state
    clients[id].isYou = true;
    send_state_to(ws);
    clients[id].isYou = false;
    
    // Acknowledge anyway after a while to avoid freezing the game
    setTimeout(function(){acknowledge_client({id:id}, null, null)}, dropAckTimeout);
    
});

server.listen(port, function() {
	log('=== LITESTRIKERS SERVER ===');
    log('Listening on port '+port);
});

setInterval(function(){
    log(Object.values(clients).length+" players currently active on the server");
}, 30000);

function send_state_to(ws){
    ws.send(PROTOCOL_STATE+get_serialized_state());
}

function get_serialized_state(){
    return JSON.stringify(get_state());
}

function get_state(){
	return {
        clients: Object.values(clients).map(get_stripped_client),
        scores: clientsScores,
		map: "default"
	}
}

function get_stripped_client(client)
{
	return {
                id: client.id,
				isSpawned: client.isSpawned,
                position: client.position,
                rotation: client.rotation,
                isYou: client.isYou,
				loadout: client.loadout,
				color: client.color
            };
}

function get_spawnpoint(){
    let randomPoint = {x: arenaSize/2, z:arenaSize/2};
    let isOk = false;
    
    while(!isOk){
        isOk = get_nearest_distance_with_any_player(randomPoint) > spawnDistance;
        
        if (!isOk){
            randomPoint = {
                x: (Math.random()*2 - 1) * arenaSize,
                z: (Math.random()*2 - 1) * arenaSize,
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
        position: {x:0, y:0, z:0},
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
						directionVector.x * directionVector.x +
						directionVector.y * directionVector.y +
						directionVector.z * directionVector.z
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

function request_spawn(me, ws, data)
{
	me.isSpawned = true;
	me.position = get_spawnpoint();
	
	for(clientId in clients)
	{ 
		if (clientId != me.id)
		{
			clients[clientId].socket.send(PROTOCOL_SPAWN_PLAYER+JSON.stringify(get_stripped_client(me)));
		}
	}
}

function switch_my_loadout(me, ws, data)
{
	const loadout = data.loadout;
	
	if (loadout < Object.keys(LOADOUTS).length -1)
	{
		me.loadout = loadout;
	
		for(clientId in clients)
		{ 
			if (clientId != me.id)
			{
				clients[clientId].socket.send(PROTOCOL_UPDATE_PLAYER+JSON.stringify(get_stripped_client(me)));
			}
		}
	}
	else
	{
		log("Received garbage loadout "+loadout+" from client "+me.id);
	}
}

function send_disconnect_player(id){
    for(clientId in clients){ 
        clients[clientId].socket.send(PROTOCOL_DISCONNECT_PLAYER+id);
    }
}

function eliminate_myself(me, ws, data){
		
	const killerID = data.killer;
	
	if (!clientsScores[killerID]){
		clientsScores[killerID] = 0;
	}
	
	clientsScores[killerID]++;
	
	me.isSpawned = false;
		
    for(clientId in clients){ 
        clients[clientId].socket.send(PROTOCOL_UPDATE_SCORE+JSON.stringify(
			{
				scores: clientScores,
				playerUpdates: [
					get_stripped_client(me)
				]
			}
		));
    }
}

function acknowledge_client(me, ws, data){
    const index = clientsAwaitingAck.indexOf(me.id);
    if (index > -1) {
        clientsAwaitingAck.splice(index, 1);
    }
}
