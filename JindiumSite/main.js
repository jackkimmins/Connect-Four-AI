let currentPlayer = 2;
let gameEnded = false;
let waitingForMove = false;
let movesCount = 0;

const audio = new Audio('./audio/1.mp3');

const PlayAudio = () => {
    setTimeout(() => {       
        audio.play();
    }, 950);
}

const CreateCells = (board = []) => {
    $('#grid').empty();

    for (let i = 0; i < 42; i++)
    {
        $('<div>').addClass('cell').attr('data-col', i % 7).attr('data-row', Math.floor(i / 7)).appendTo('#grid');
    }
}

const GetColNum = (cell) => {
    return parseInt(cell.getAttribute('data-col'));
}

//Returns the 4 cells that make up a winning line
const GetWinningCells = () => {
    let winningCells = [];

    //check horizontal
    for (let i = 0; i < 6; i++)
    {
        for (let j = 0; j < 4; j++)
        {
            let cells = $(`.cell[data-row="${i}"][data-col="${j}"], .cell[data-row="${i}"][data-col="${j+1}"], .cell[data-row="${i}"][data-col="${j+2}"], .cell[data-row="${i}"][data-col="${j+3}"]`);
            let cellsArr = cells.toArray();
            if (cellsArr.every(c => c.classList.contains('player1')) || cellsArr.every(c => c.classList.contains('player2')))
            {
                winningCells = cellsArr;
                break;
            }
        }
    }

    //check vertical
    for (let i = 0; i < 3; i++)
    {
        for (let j = 0; j < 7; j++)
        {
            let cells = $(`.cell[data-row="${i}"][data-col="${j}"], .cell[data-row="${i+1}"][data-col="${j}"], .cell[data-row="${i+2}"][data-col="${j}"], .cell[data-row="${i+3}"][data-col="${j}"]`);
            let cellsArr = cells.toArray();
            if (cellsArr.every(c => c.classList.contains('player1')) || cellsArr.every(c => c.classList.contains('player2')))
            {
                winningCells = cellsArr;
                break;
            }
        }
    }

    //check diagonal
    for (let i = 0; i < 3; i++)
    {
        for (let j = 0; j < 4; j++)
        {
            let cells = $(`.cell[data-row="${i}"][data-col="${j}"], .cell[data-row="${i+1}"][data-col="${j+1}"], .cell[data-row="${i+2}"][data-col="${j+2}"], .cell[data-row="${i+3}"][data-col="${j+3}"]`);
            let cellsArr = cells.toArray();
            if (cellsArr.every(c => c.classList.contains('player1')) || cellsArr.every(c => c.classList.contains('player2')))
            {
                winningCells = cellsArr;
                break;
            }
        }
    }

    //check diagonal
    for (let i = 0; i < 3; i++)
    {
        for (let j = 3; j < 7; j++)
        {
            let cells = $(`.cell[data-row="${i}"][data-col="${j}"], .cell[data-row="${i+1}"][data-col="${j-1}"], .cell[data-row="${i+2}"][data-col="${j-2}"], .cell[data-row="${i+3}"][data-col="${j-3}"]`);
            let cellsArr = cells.toArray();
            if (cellsArr.every(c => c.classList.contains('player1')) || cellsArr.every(c => c.classList.contains('player2')))
            {
                winningCells = cellsArr;
                break;
            }
        }
    }

    return winningCells;
}


$('#grid').on('mouseenter', '.cell', function() {
    if (gameEnded) return;

    const colNum = GetColNum(this);
    const col = $(`.cell[data-col="${colNum}"]`);
    const emptyCell = col.filter((i, c) => c.childElementCount === 0);

    for (let i = 0; i < emptyCell.length; i++)
    {
        //don't apply hover if the cell is already occupied
        if (emptyCell[i].classList.contains('player1') || emptyCell[i].classList.contains('player2')) continue;

        emptyCell[i].classList.add('hover');
    }
});

$('#grid').on('mouseleave', '.cell', function() {
    $('.cell').removeClass('hover');
});

function SwitchTurn()
{
    currentPlayer = currentPlayer === 1 ? 2 : 1;
    
    $('#turn').text(currentPlayer === 1 ? 'Make Your Move' : 'Computer is Thinking');
    $('#turnColour').css('background-color', `var(--player${currentPlayer})`);
}

function PlaceCell(colNum)
{
    let col = $(`[data-col=${colNum}]`);
    let colCells = col.toArray().reverse();
    for (let i = 0; i < colCells.length; i++)
    {
        let cell = colCells[i];
        if (!cell.classList.contains('player1') && !cell.classList.contains('player2'))
        {
            cell.classList.add(`player${currentPlayer}`);
            cell.setAttribute('data-player', currentPlayer);
            break;
        }
    }

    //remove hover class
    $('.cell').removeClass('hover');

    PlayAudio();
    SwitchTurn();
}

function AddMove(column, player, numIterations = null, score = null)
{
    //if there are more than 7 rows, then remove the last row in the table
    if (movesCount > 5)
    {
        $('#moves tbody tr:last').remove();
    }


    let row = `<tr><td>#${movesCount+1}</td>
                   <td>${column}</td>
                   <td style="color: var(--player${player})">${player === 1 ? 'Player' : 'Computer'}</td>
                   <td>${numIterations ? numIterations.toLocaleString() : 'N/A'}</td>
                   <td>${score ? score : 'N/A'}</td>
                </tr>`;

    $('#moves tbody').prepend(row);

    movesCount++;
}

$('#grid').click(function(e) {
    if (gameEnded || waitingForMove) return;

    const cell = e.target.closest('.cell');

    if (cell)
    {
        const colNum = GetColNum(cell);
        console.log(colNum);
        AddMove(colNum + 1, currentPlayer);
        PlaceCell(colNum);

        let winningMsg = '';

        $.ajax({
            url: './dropToken',
            type: 'POST',
            data: { col: colNum + 1 },
            beforeSend: function() {
                waitingForMove = true;
            },
            statusCode: {
                400: function (res) {
                    alert(res);
                },
                201: function (res) {
                    let json = JSON.parse(res);
                    console.log(json);
                    AddMove(json.Column, currentPlayer, json.Iterations, json.Score);
                    PlaceCell(json.Column - 1);
                },
                200: function (res) {
                    let json = JSON.parse(res);
                    AddMove(json.Column, currentPlayer, json.Iterations, json.Score);
                    PlaceCell(json.Column - 1);
                    gameEnded = true;
                    winningMsg = 'You lost!';
                },
                203: function (res) {
                    gameEnded = true;
                    winningMsg = 'You won!';
                },
                406: function (res) {
                    NewGame();
                }
            },
            complete: function() {
                waitingForMove = false;
                if (gameEnded)
                {
                    if (winningMsg === 'You lost!')
                    {
                        new Audio('./audio/loss.mp3').play();
                    }
                    else
                    {
                        new Audio('./audio/win.mp3').play();
                    }

                    const winningCells = GetWinningCells();
                    for (let i = 0; i < winningCells.length; i++)
                    {
                        winningCells[i].classList.add('winning');
                    }

                    $('#turn').text(winningMsg);
                }
            }
        });
    }

});

function NewGame()
{
    $.ajax({
        url: './newGame',
        type: 'POST',
        success: function(res) {
            console.log(res);
        }
    });

    gameEnded = false;
    currentPlayer = 2;
    SwitchTurn();
    waitingForMove = false;
    movesCount = 0;

    CreateCells();
}

$('#reset').click(function() {
    NewGame();
});

//On dom load
$(document).ready(function() {
    NewGame();
});