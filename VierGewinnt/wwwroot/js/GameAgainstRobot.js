"use strict";

var connection = new signalR.HubConnectionBuilder().withUrl("/gameHub").build();

// Prepare Game
connection.start()
    .then(() => {
        PlaceAlreadyPlayedMoves(movesToLoad); 
        // Wenn playerOne immer Rot ist, dann heisst das, dass PlayerOne immer bei ungeraden Zahlen drann ist.
        connection.invoke("RegisterGameInStaticProperty", playerOneID, playerTwoID, gameId)
    }
);

function PlaceAlreadyPlayedMoves(movesToLoad) {
    event.preventDefault();
    let colDepth = {};

    colDepth['1'] = 6;
    colDepth['2'] = 6;
    colDepth['3'] = 6;
    colDepth['4'] = 6;
    colDepth['5'] = 6;
    colDepth['6'] = 6;
    colDepth['7'] = 6;

    var moveNr = 1;
    
    movesToLoad.forEach(m => {
        var column = m['column'];
        var key = `${column}`;
        
        if (moveNr % 2 == 0) {
            var selectedCell = document.getElementById(`${column}${colDepth[key]}`);
            if (selectedCell != null) {
                selectedCell.style.backgroundColor = "yellow";
                array[column -1][(colDepth[key]) -1] = 1; // update virtual board
                colDepth[key] = colDepth[key] - 1;
                moveNr++;
                return;
            }
        } else {
            var selectedCell = document.getElementById(`${column}${colDepth[key]}`);
            if (selectedCell != null) {
                selectedCell.style.backgroundColor = "red";
                array[column -1][(colDepth[key]) -1] = 1; // update virtual board
                colDepth[key] = colDepth[key] - 1;
                moveNr++;
                return;
            }
        }    
    });


    // To Do: activateButton not necessary for AI move.....

    if (moveNr % 2 != 0) {
        activateButton("btnColRed");
        disableButton("btnColYellow");
    } else  {
        activateButton("btnColYellow");
        disableButton("btnColRed");
    }
}


// Player action
window.onload = function () {

    // To Do: implement robot move instead of yellow.......

    var YellowBtn = document.getElementById("btnColYellow");
    YellowBtn.onclick = function () {
        event.preventDefault();
        disableButton("btnColYellow");
        var arr = $('form').serializeArray();
        var dataObj = {};

        $(arr).each(function (i, field) {
            dataObj[field.name] = field.value;
        });

        var playerIdOne = dataObj['userIdTwo'];
        var boardIdOne = dataObj['boardIdTwo'];
        var columnYellow = dataObj['colNumberYel'];

        connection.invoke("SendPlayerMove", playerIdOne, boardIdOne, columnYellow);
    }


    var RedBtn = document.getElementById("btnColRed");
    RedBtn.onclick = function () {
        event.preventDefault();
        disableButton("btnColRed");
        var arr = $('form').serializeArray();
        var dataObj = {};

        $(arr).each(function (i, field) {
            dataObj[field.name] = field.value;
        });

        var playerIdTwo = dataObj['userIdOne'];
        var boardIdTwo = dataObj['boardIdOne'];
        var columnRed = dataObj['colNumberRed'];

        connection.invoke("SendPlayerMove", playerIdTwo, boardIdTwo, columnRed);
    }
}

// Robot action
function startAiMove() {

}


// animate Moves
connection.on("AnimatePlayerMove", async (column, playerId) => {
    event.preventDefault();
    var playerIdOne = document.getElementById("playerIdOne").value;
    var playerIdTwo = document.getElementById("playerIdTwo").value; // playerIdTwo correct for AI?
    var endRow = await PlaceChipInArray(column)
    if (endRow == "full") {
        alert("Row is already full. Please select another Row.")
        if (playerIdOne == playerId) {
            activateButton("btnColRed")
        }
        else {
            activateButton("btnColYellow")
        }
    }
    else if (playerIdOne == playerId) {
        animate(column, endRow, "red")
    }
    else if (playerIdTwo == playerId) {
        animate(column, endRow, "yellow")
    }
});

async function animate(column, endRow, color) {
    if (color == "yellow") {
        for (let row = 1; row <= endRow; row++) {
            var selectedCell = document.getElementById(`${column}${row}`);
            if (selectedCell != null) {
                if (row == endRow) {
                    selectedCell.style.backgroundColor = "yellow";
                    activateButton("btnColRed");
                    return;
                }
                selectedCell.classList.add('blinkYellow');
                await new Promise(resolve => setTimeout(resolve, 200));
                selectedCell.classList.remove('blinkYellow');
 
            }
        }
    }
    else if (color == "red") {
        for (let row = 1; row <= endRow; row++) {
            var selectedCell = document.getElementById(`${column}${row}`);
            if (selectedCell != null) {
                if (row == endRow) {
                    selectedCell.style.backgroundColor = "red";
                    activateButton("btnColYellow");
                    return;
                }
                selectedCell.classList.add('blinkRed');
                await new Promise(resolve => setTimeout(resolve, 200));
                selectedCell.classList.remove('blinkRed');
            }
        }
    }
}

//virtual board to check if a chip is already placed
var array = [
    [0, 0, 0, 0, 0, 0],
    [0, 0, 0, 0, 0, 0],
    [0, 0, 0, 0, 0, 0],
    [0, 0, 0, 0, 0, 0],
    [0, 0, 0, 0, 0, 0],
    [0, 0, 0, 0, 0, 0],
    [0, 0, 0, 0, 0, 0],
];

async function PlaceChipInArray(column) {
    var endRow = await GetEndRow(column);
    if (endRow == "full") {
        return "full"
    }
    array[column - 1][endRow - 1] = 1
    return endRow
}

async function GetEndRow(column) {
    for (var i = 6; i >= 0; i--) {
        if (i == 0) {
            return "full"
        }
        else if (array[column - 1][i - 1] == 0) { // column and row of the board starts at 1, array starts at 0, so we subtract - 1 
            return i
        }
    }
}

function disableButton(btnId) {
    var button = document.getElementById(btnId);
    button.disabled = true;
}

function activateButton(btnId) {
    var button = document.getElementById(btnId);
    button.disabled = false;
}


// Game End
connection.on("NotificateGameEnd", function (winnerId) {
    console.log(`Gratuliere ${winnerId}!! Du hast gewonnen!`);
});

