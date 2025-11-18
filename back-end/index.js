document.addEventListener('DOMContentLoaded', () => {
    const canvas = document.getElementById("puzzleCanvas");
    const ctx = canvas.getContext("2d");
    const inputs = document.getElementById('inputs');
    const newGameBtn = document.getElementById('new-game-button');
    const clearBtn = document.getElementById('clear-button');
    const hintBtn = document.getElementById('hint-button');
    const checkBtn = document.getElementById('check-button');
    const difficulty = document.getElementById('difficulty');
    const showTimer = document.getElementById('timer');

    const numOfGrid = 9;
    const size = canvas.width; // assume square
    const cell = size / numOfGrid;

    let solution = null; 
    let puzzle = null; // puzzle shown to user (0 for empty)
    let timer = null;
    let seconds = 0;
    let lastFocusedCell = null;

    //////////////////////////////////////////////////////////////////////////////////
    //                      GRID GRAWING
    //////////////////////////////////////////////////////////////////////////////////
    // Functions to draw grids
    function drawGrids(){
        ctx.clearRect(0, 0, size, size);  //clear the canvas
        ctx.fillStyle = '#FCF3BB';      //background of the boxes
        ctx.fillRect(0, 0, size, size);

        ctx.strokeStyle = '#BBB';       //colour of thinner border
        ctx.lineWidth = 2;                 //width of thinner border

        for(let i=0; i<numOfGrid; i++){
            ctx.beginPath();
            ctx.moveTo(0, i*cell);          //move to top left corner of the 9 mini grids in first column
            ctx.lineTo(size, i*cell);       //draw the line to size of the canvas (width)
            ctx.stroke();

            ctx.beginPath();
            ctx.moveTo(i*cell,0);           //move to top right corner of the 9 mini grids in the first row
            ctx.lineTo(i*cell,size);        //draw the line to size of the canvas (height)
            ctx.stroke();
        }

        ctx.beginPath();
        ctx.strokeStyle = '#000';       //color of thicker border
        ctx.lineWidth = 3;                 //width of thicker border
        
        for(let i=0;i<=numOfGrid;i+=3){
            ctx.beginPath();
            ctx.moveTo(0,i*cell);           //move to the top left corner of 3 grids in first column
            ctx.lineTo(size,i*cell);        //draw the line to size of the canvas (width)
            ctx.stroke();

            ctx.beginPath();
            ctx.moveTo(i*cell,0);           //move to the top right corner of 3 grids in first row
            ctx.lineTo(i*cell,size);        //draw the line to size of the canvas (height)
            ctx.stroke();
        }
    }
    ////////////////////////////////////////////////////////////////////////////////
    //                    TIMER
    ////////////////////////////////////////////////////////////////////////////////
    //Function to start the timer
    function startTimer(){
        stopTimer();
        seconds = 0;
        showTimer.textContent = formatTime(seconds);
        timer = setInterval(()=>{
            seconds++;
            showTimer.textContent = formatTime(seconds);
        }, 1000)
    }

    //Function to stop the timer
    function stopTimer(){
        if(timer){
            clearInterval(timer);
        }else{
            timer = null;
        }
    }

    //Function to format time to 00:00
    function formatTime(s){ 
        const min = Math.floor(s/60); 
        const sec = s%60; 
        return String(min).padStart(2,'0')+":"+String(sec).padStart(2,'0'); 
    }
    //////////////////////////////////////////////////////////////////////////////////
    //                      INPUT
    //////////////////////////////////////////////////////////////////////////////////
    //Build input for the grid
    function buildInput(){
        inputs.innerHTML = '';
        for(let row=0; row<numOfGrid; row++){
            for(let col=0; col<numOfGrid; col++){
                const idx = row*9+col;
                const input = document.createElement('input');
                input.type = 'text';
                input.maxLength = 1;
                input.dataset.r = row;
                input.dataset.c = col;
                input.autocomplete = 'off';
                input.addEventListener('input', onInput);
                input.addEventListener('keydown', onKeyDown);
                inputs.appendChild(input);
            }
        }

    }

    function onKeyDown(e){
        // Block all number input from keyboard
        if(/[0-9]/.test(e.key)){
            e.preventDefault();
            return;
        }
        // allow navigation with arrow keys
        const input = e.target;
        const r = parseInt(input.dataset.r,10);
        const c = parseInt(input.dataset.c,10);
        if(['ArrowLeft','ArrowRight','ArrowUp','ArrowDown'].includes(e.key)){
            e.preventDefault();
            let nr=r,nc=c;
            if(e.key==='ArrowLeft') nc=Math.max(0,c-1);
            if(e.key==='ArrowRight') nc=Math.min(8,c+1);
            if(e.key==='ArrowUp') nr=Math.max(0,r-1);
            if(e.key==='ArrowDown') nr=Math.min(8,r+1);
            const pos = nr*9+nc;
            inputs.children[pos].focus();
        }
    }

    function onInput(e){
        const v = e.target.value.replace(/[^1-9]/g,'');
        e.target.value = v;
    }
    //////////////////////////////////////////////////////////////////////////////////
    //                      CREATING PUZZLE BOARD
     //////////////////////////////////////////////////////////////////////////////////
    //Function to find which cell is empty 
    function findEmptyCell(board){
        for(let row=0; row<numOfGrid; row++){
            for(let col=0; col<numOfGrid; col++){
                if(board[row][col] === 0){
                    return [row, col];
                }
                
            }
        }
        return null;
    }

    //Function to shuffle all the numbers in the board
    function shuffle(nums){
        for(let i=nums.length-1; i>0; i--){
            const j=Math.floor(Math.random()*(i+1));
            [nums[i],nums[j]]=[nums[j],nums[i]];
        }
    }

    //Function to determine whether the row or column contain the number before
    function isSafe(board, row, col, num){
        //check whether the number exist in columns
        for(let c=0; c<numOfGrid; c++){
            if(board[row][c]===num){
                return false;
            }
        }

        //check whether the number exist in rows
        for(let r=0; r<numOfGrid; r++){
            if(board[r][col]===num){
                return false;
            } 
        } 
        const sr = Math.floor(row/3)*3; 
        const sc = Math.floor(col/3)*3;
        //find 3x3 subgrid and check all the 9 cellss within
        for(let r=sr; r<sr+3; r++){
            for(let c=sc;c<sc+3;c++){
                if(board[r][c]===num){
                    return false;
                } 
            } 
        } 
        return true;
    }

    //Function to solve the board
    function solveBoard(board){
        const emp = findEmptyCell(board);       //search for empty grid
        if(!emp){
            return true;
        }
        const [row, col] = emp;
        const nums = [1, 2, 3, 4, 5, 6, 7, 8, 9];
        shuffle(nums);
        for(const i of nums){
            if(isSafe(board, row, col, i)){
                board[row][col] = i;
                if(solveBoard(board)){
                    return true;
                }
                board[row][col] = 0;
            }
        }
        return false;
    }

    //Function to generate the full board with solution 
    function generateBoardWithSolution(){
        const board = [];

        //fill 0 in every cell
        for (let i = 0; i < 9; i++) {
            const row = [];
            for (let j = 0; j < 9; j++) {
                row.push(0);
            }
            board.push(row);
        }
        solveBoard(board);
        return board;
    }

    //Function to copy board
    function copyBoard(b){ 
        return b.map(r=>r.slice()); 
    }

    //Function to create puzzle by removing some numbers randomly
    function createPuzzle(board, numToRemove){
        const brd = copyBoard(board);
        const cells = [];
        for(let row=0; row<numOfGrid; row++){
            for(let col=0; col<numOfGrid; col++){
                cells.push([row, col]);
            }
        }
        shuffle(cells);
        let removed = 0;
        const totalToRemoved = numToRemove;
        for(const[row, col] of cells){
            if(removed >= totalToRemoved){
                break;
            }
            const backup = brd[row][col];
            brd[row][col] = 0;
            removed++;
        }
        return brd;
    }

    //Function to populate puzzle
    function populatePuzzle(board){
        for(let row=0; row<numOfGrid; row++){
            for(let col=0; col<numOfGrid; col++){
                const inpt = inputs.children[row*9 + col];
                const val = board[row][col];

                inpt.classList.remove('given');
                inpt.classList.remove('clue');
                inpt.classList.remove('invalid');
                inpt.value = '';

                if(val !== 0){                          //if cell is not empty, marked as given
                    inpt.value = String(val);
                    inpt.classList.add('given');
                    inpt.readOnly = true;
                }else{
                    inpt.readOnly = false;              //if cell is empty, make it editable
                }
            }
        }
    }
    //////////////////////////////////////////////////////////////////////////////////
    //                      BUTTON FUNCTIONS
    //////////////////////////////////////////////////////////////////////////////////
    //Get the current board situation
    function getCurrentBoard(){
        const b = Array.from({length:9},()=>Array(9).fill(0));
        for(let row=0; row<numOfGrid; row++){
            for(let col=0; col<numOfGrid; col++){
                const v = inputs.children[row*9 + col].value;
                b[row][col] = v? parseInt(v,10):0;
            }
        }
        return b;
    }

    //Check if the user has input the wrong answer
    function checkPuzzle(){
        const user = getCurrentBoard();
        let allOk = true;
        // clear previous invalids
        for(const inpt of inputs.children){
            inpt.classList.remove('invalid');
        } 
        for(let row=0; row<numOfGrid; row++){
            for(let col=0; col<numOfGrid; col++){
                const val = user[row][col];
                if(val===0) { 
                    allOk=false; continue; 
                }
                if(val !== solution[row][col]){
                    allOk=false;
                    inputs.children[row*9 + col].classList.add('invalid');
                }
            }
        }
        if(allOk){ 
            stopTimer(); 
            alert('Congratulations! You solved the puzzle in ' + formatTime(seconds) + '!');
        } else {
            alert('Wrong cells are highlighted in red.');
        }
    }

    //New Game button
    newGameBtn.addEventListener('click', ()=>{
        startGame();
    });

    // Track which cell was focused before number pad button is pressed
    inputs.addEventListener('blur', (e)=>{
        if(e.target && e.target.dataset.r !== undefined){
            lastFocusedCell = e.target;
        }
    }, true);

    //Check Button
    checkBtn.addEventListener('click', checkPuzzle);

    //Hint Button
    hintBtn.addEventListener('click', (e)=>{ 
        e.preventDefault();
        if(lastFocusedCell && lastFocusedCell.dataset.r !== undefined){
            const row = parseInt(lastFocusedCell.dataset.r,10);
            const col = parseInt(lastFocusedCell.dataset.c,10);
            lastFocusedCell.value = String(solution[row][col]);
            lastFocusedCell.focus();
        }
    });

    //Number pad button
    document.querySelectorAll('.number-button').forEach(btn=>{
        btn.addEventListener('click', (e)=>{
        if(lastFocusedCell && lastFocusedCell.dataset.r !== undefined){
            lastFocusedCell.value = e.target.dataset.number;
            lastFocusedCell.focus();
        }
        });
    });

    //Clear Button
    clearBtn.addEventListener('click', ()=>{
        if(lastFocusedCell && lastFocusedCell.dataset.r !== undefined && !lastFocusedCell.readOnly){
        lastFocusedCell.value = '';
        lastFocusedCell.focus();
        }
    });
    //////////////////////////////////////////////////////////////////////////////////
    //                      START GAME
    //////////////////////////////////////////////////////////////////////////////////
    //Function to start the game
    function startGame(){
        drawGrids();
        startTimer();
        const full = generateBoardWithSolution();
        solution = full;
        const numToRemove = parseInt(difficulty.value,10);       //remove number based on the difficulty
        puzzle = createPuzzle(copyBoard(full), numToRemove);
        populatePuzzle(puzzle);
    }

    //run these functions when opening the web;
    buildInput();
    drawGrids();
    startGame();

});