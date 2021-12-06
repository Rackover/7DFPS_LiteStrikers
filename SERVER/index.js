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

const loadouts
{
	LMG:1
	TRIPLE:2,
	HOMING:3
}

const PROTOCOL_MOVESTATE = "MOV";
const PROTOCOL_UPDATE_SCORE = "SCO";
const PROTOCOL_STATE = "STT";
const PROTOCOL_ACKNOWLEDGE_STATE = "AKS";
const PROTOCOL_KILL_PLAYER = "KIL";
const PROTOCOL_SHOOTSTATE = "SHT";
const PROTOCOL_SET_LOADOUT = "LDT";
const PROTOCOL_MEOW = "MEW";

const handlers = {};
handlers[PROTOCOL_MOVESTATE] = send_position_to_everyone_but_me;
handlers[PROTOCOL_UPDATE_SCORE]= function(me, ws, data){ log("someone ("+me.id+") sent me a PROTOCOL_UPDATE_SCORE ??");};
handlers[PROTOCOL_ACKNOWLEDGE_STATE]=  acknowledge_client;
handlers[PROTOCOL_STATE]= function(me, ws, data){ log("someone ("+me.id+") sent me a PROTOCOL_STATE ??");};
handlers[PROTOCOL_KILL_PLAYER]= function(me, ws, data){ log("someone ("+me.id+") sent me a PROTOCOL_KILL_PLAYER ??");};
handlers[PROTOCOL_MEOW]= send_meow_to_everyone;
handlers[PROTOCOL_SHOOTSTATE]= send_shoot_to_everyone_but_me;
handlers[PROTOCOL_SET_LOADOUT]= switch_my_loadout;

let clients = {};
let idCounter = 0;
let clientsAwaitingAck = [];
let clientsScores = {};


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
        send_kill_player(id);   
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
                position: client.position,
                rotation: client.rotation,
                isYou: client.isYou
            };
        }),
        scores: clientsScores
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
    return {
        id:id,
        socket:ws,
        position: get_spawnpoint(),
        rotation: [0, 0, 0, 0],
        loadout: Math.floor(Math.random()*loadouts.length)
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
    for(client_id in clients){
        if (client_id == me.id){
            let parsed = JSON.parse(data);
            clients[client_id].position = parsed.position;
            clients[client_id].isSneaking = parsed.isSneaking;
            continue;            
        }
        
        clients[client_id].socket.send(PROTOCOL_MOVESTATE+data);
    }
}

function send_caught_to_everyone_but_me_and_increase_score(me, ws, data){
    try{
        log("Client "+me.id+" caught bird "+data);
        
        
        if (!clientsScores[me.id]){
            clientsScores[me.id] = 0;
        }
        
        clientsScores[me.id]++;
        
        for(client_id in clients){            
            clients[client_id].socket.send(PROTOCOL_UPDATE_SCORE+JSON.stringify({
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
    for(client_id in clients)
	{            
        clients[client_id].socket.send(PROTOCOL_MEOW+me.id);
    }
}

function send_shoot_to_everyone_but_me(me, ws, data)
{
	
}

function switch_my_loadout(me, ws, data)
{
	const loadout = data.loadout;
	
}

function send_kill_player(id){
    for(client_id in clients){            
        clients[client_id].socket.send(PROTOCOL_KILL_PLAYER+id);
    }
}

function acknowledge_client(me, ws, data){
    const index = clientsAwaitingAck.indexOf(me.id);
    if (index > -1) {
        clientsAwaitingAck.splice(index, 1);
    }
}

