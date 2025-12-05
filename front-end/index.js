document.addEventListener('DOMContentLoaded', () => {
    console.log('=== Sudoku Game Loaded v4 ===');
    
    const canvas = document.getElementById("puzzleCanvas");
    const ctx = canvas.getContext("2d");
    const inputs = document.getElementById('inputs');
    const newGameBtn = document.getElementById('new-game-button');
    const clearBtn = document.getElementById('clear-button');
    const hintBtn = document.getElementById('hint-button');
    const checkBtn = document.getElementById('check-button');
    const pauseBtn = document.getElementById('pause-button');
    const startButton = document.getElementById('startButton');
    const startOverlay = document.getElementById('startOverlay');
    const puzzleBoard = document.getElementById('puzzleBoard');
    const difficulty = document.getElementById('difficulty');
    const showTimer = document.getElementById('timer');

    console.log('Game elements:', {
        pauseBtn: !!pauseBtn,
        startButton: !!startButton,
        startOverlay: !!startOverlay
    });

    // Authentication elements
    const usernameInput = document.getElementById('username');
    const passwordInput = document.getElementById('password');
    const registerBtn = document.getElementById('registerBtn');
    const loginBtn = document.getElementById('loginBtn');
    const logoutBtn = document.getElementById('logoutBtn');
    const authStatus = document.getElementById('authStatus');

    console.log('Auth elements loaded:', {
        usernameInput: !!usernameInput,
        passwordInput: !!passwordInput,
        registerBtn: !!registerBtn,
        loginBtn: !!loginBtn,
        logoutBtn: !!logoutBtn,
        authStatus: !!authStatus
    });

    // const sudoku = require('./logic');
    
    // Authentication state
    let authToken = localStorage.getItem('authToken');
    let refreshToken = localStorage.getItem('refreshToken');
    let currentUsername = localStorage.getItem('username');

    // Game state
    let gameStarted = false;
    let gamePaused = false;

    const numOfGrid = 9;
    const size = canvas.width; // assume square
    const cell = size / numOfGrid;

    let solution = null; 
    let puzzle = null; // puzzle shown to user (0 for empty)
    let timer = null;
    let seconds = 0;
    let hintsUsed = 0; // Track hints used
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
        if (!gameStarted || gamePaused) return; // Don't start if game not started or paused
        stopTimer();
        seconds = 0;
        showTimer.textContent = formatTime(seconds);
        timer = setInterval(()=>{
            if (!gamePaused) { // Only increment if not paused
                seconds++;
                showTimer.textContent = formatTime(seconds);
            }
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

    //Function to pause the timer
    function pauseTimer(){
        if(timer){
            clearInterval(timer);
            timer = null;
        }
    }

    //Function to resume the timer
    function resumeTimer(){
        if (!timer && gameStarted) {
            timer = setInterval(()=>{
                if (!gamePaused) {
                    seconds++;
                    showTimer.textContent = formatTime(seconds);
                }
            }, 1000);
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
        
        // Auto-check if puzzle is complete
        if (gameStarted && !gamePaused) {
            autoCheckCompletion();
        }
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

    //Auto-check completion after each input
    async function autoCheckCompletion(){
        const user = getCurrentBoard();
        let allFilled = true;
        let allCorrect = true;
        
        for(let row=0; row<numOfGrid; row++){
            for(let col=0; col<numOfGrid; col++){
                const val = user[row][col];
                if(val===0) { 
                    allFilled=false; 
                    break;
                }
                if(val !== solution[row][col]){
                    allCorrect=false;
                }
            }
            if(!allFilled) break;
        }
        
        if(allFilled && allCorrect){ 
            stopTimer(); 
            gameStarted = false;
            
            // Show completion banner
            const banner = document.getElementById('completionBanner');
            banner.style.display = 'block';
            
            // Save game to database
            await saveCompletedGame();
            
            // Reload performance stats
            await loadPerformanceStats();
            
            setTimeout(() => {
                alert('Congratulations! You solved the puzzle in ' + formatTime(seconds) + '!');
            }, 100);
        }
    }

    //Check if the user has input the wrong answer (manual check button)
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
            gameStarted = false;
            const banner = document.getElementById('completionBanner');
            banner.style.display = 'block';
            
            // Save game to database
            saveCompletedGame();
            
            // Reload performance stats
            loadPerformanceStats();
            
            alert('Congratulations! You solved the puzzle in ' + formatTime(seconds) + '!');
        } else {
            alert('Wrong cells are highlighted in red.');
        }
    }

    // Save completed game to database
    async function saveCompletedGame(){
        if (!authToken) {
            console.log('User not logged in, game not saved');
            return;
        }

        const difficultyValue = parseInt(difficulty.value, 10);
        let difficultyName = 'easy';
        if (difficultyValue >= 56) difficultyName = 'hard';
        else if (difficultyValue >= 50) difficultyName = 'medium';

        const puzzleString = puzzle.map(row => row.join('')).join('');
        const solutionString = solution.map(row => row.join('')).join('');

        try {
            const response = await fetch('/api/games', {
                method: 'POST',
                headers: { 
                    'Content-Type': 'application/json',
                    'Authorization': `Bearer ${authToken}`
                },
                body: JSON.stringify({
                    puzzle: puzzleString,
                    solution: solutionString,
                    elapsedSeconds: seconds,
                    difficulty: difficultyName,
                    isCompleted: true,
                    lastPlayedAt: new Date().toISOString(),
                    name: `${difficultyName} - ${formatTime(seconds)}`,
                    hintsUsed: hintsUsed
                })
            });

            if (response.ok) {
                console.log('Game saved successfully');
            } else if (response.status === 401) {
                console.log('Unauthorized - token may have expired');
            } else {
                console.log('Failed to save game:', response.statusText);
            }
        } catch (err) {
            console.error('Error saving game:', err);
        }
    }

    // Load and display performance stats
    async function loadPerformanceStats(){
        // If user is logged in, load their personal stats
        if (authToken) {
            try {
                const response = await fetch('/api/games', {
                    headers: { 
                        'Authorization': `Bearer ${authToken}`
                    }
                });

                if (!response.ok) {
                    console.log('Failed to load stats, loading general stats instead');
                    loadGeneralStats();
                    return;
                }

                const games = await response.json();
                const completedGames = games.filter(g => g.isCompleted);

                // Calculate stats by difficulty
                const stats = {
                    easy: { times: [], hints: [], count: 0 },
                    medium: { times: [], hints: [], count: 0 },
                    hard: { times: [], hints: [], count: 0 }
                };

                completedGames.forEach(game => {
                    const diff = game.difficulty || 'easy';
                    if (stats[diff]) {
                        stats[diff].times.push(game.elapsedSeconds);
                        stats[diff].hints.push(game.hintsUsed || 0);
                        stats[diff].count++;
                    }
                });

                // Update dashboard
                ['easy', 'medium', 'hard'].forEach(diff => {
                    const row = document.querySelector(`tr[data-diff="${diff}"]`);
                    if (!row) return;

                    const data = stats[diff];
                    const bestCell = row.querySelector('[data-field="best"]');
                    const avgCell = row.querySelector('[data-field="avg"]');
                    const totalHintsCell = row.querySelector('[data-field="totalHints"]');
                    const avgHintsCell = row.querySelector('[data-field="avgHints"]');

                    if (data.times.length > 0) {
                        const best = Math.min(...data.times);
                        const avg = data.times.reduce((a,b) => a+b, 0) / data.times.length;
                        const totalHints = data.hints.reduce((a,b) => a+b, 0);
                        const avgHints = totalHints / data.hints.length;
                        
                        bestCell.textContent = formatTime(best);
                        avgCell.textContent = formatTime(Math.round(avg));
                        totalHintsCell.textContent = totalHints;
                        avgHintsCell.textContent = avgHints.toFixed(1);
                    } else {
                        bestCell.textContent = '–';
                        avgCell.textContent = '–';
                        totalHintsCell.textContent = '–';
                        avgHintsCell.textContent = '–';
                    }
                });

            } catch (err) {
                console.error('Error loading stats:', err);
                loadGeneralStats();
            }
        } else {
            // Load general stats for non-logged-in users
            loadGeneralStats();
        }
    }

    // Load general statistics (for all users)
    async function loadGeneralStats(){
        try {
            const response = await fetch('/api/games/stats/general');

            if (!response.ok) {
                console.log('Failed to load general stats');
                return;
            }

            const stats = await response.json();

            // Convert array to object
            const statsByDiff = {};
            stats.forEach(s => {
                statsByDiff[s.difficulty] = s;
            });

            // Update dashboard with general stats
            ['easy', 'medium', 'hard'].forEach(diff => {
                const row = document.querySelector(`tr[data-diff="${diff}"]`);
                if (!row) return;

                const data = statsByDiff[diff];
                const bestCell = row.querySelector('[data-field="best"]');
                const avgCell = row.querySelector('[data-field="avg"]');
                const totalHintsCell = row.querySelector('[data-field="totalHints"]');
                const avgHintsCell = row.querySelector('[data-field="avgHints"]');

                if (data && data.totalGames > 0) {
                    bestCell.textContent = formatTime(data.bestTime);
                    avgCell.textContent = formatTime(data.avgTime);
                    totalHintsCell.textContent = data.totalHints;
                    avgHintsCell.textContent = data.avgHints.toFixed(1);
                } else {
                    bestCell.textContent = '–';
                    avgCell.textContent = '–';
                    totalHintsCell.textContent = '–';
                    avgHintsCell.textContent = '–';
                }
            });

        } catch (err) {
            console.error('Error loading general stats:', err);
        }
    }

    //New Game button
    newGameBtn.addEventListener('click', ()=>{
        startGame();
        // Reset game state
        gameStarted = false;
        gamePaused = false;
        pauseBtn.textContent = 'Pause';
        pauseBtn.style.background = '#ffc107';
        // Hide completion banner
        document.getElementById('completionBanner').style.display = 'none';
        // Show start overlay
        startOverlay.style.display = 'flex';
        inputs.style.filter = 'blur(8px)';
        canvas.style.filter = 'blur(8px)';
        inputs.style.pointerEvents = 'none';
    });

    //Start Button
    startButton.addEventListener('click', ()=>{
        gameStarted = true;
        gamePaused = false;
        startOverlay.style.display = 'none';
        inputs.style.filter = 'none';
        canvas.style.filter = 'none';
        inputs.style.pointerEvents = 'auto';
        startTimer();
    });

    //Pause Button
    pauseBtn.addEventListener('click', ()=>{
        if (!gameStarted) {
            alert('Please start the game first!');
            return;
        }
        
        gamePaused = !gamePaused;
        
        if (gamePaused) {
            // Pause the game
            pauseTimer();
            pauseBtn.textContent = 'Resume';
            pauseBtn.style.background = '#28a745';
            inputs.style.filter = 'blur(8px)';
            canvas.style.filter = 'blur(8px)';
            inputs.style.pointerEvents = 'none';
            clearBtn.disabled = true;
            hintBtn.disabled = true;
            checkBtn.disabled = true;
        } else {
            // Resume the game
            resumeTimer();
            pauseBtn.textContent = 'Pause';
            pauseBtn.style.background = '#ffc107';
            inputs.style.filter = 'none';
            canvas.style.filter = 'none';
            inputs.style.pointerEvents = 'auto';
            clearBtn.disabled = false;
            hintBtn.disabled = false;
            checkBtn.disabled = false;
        }
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
            hintsUsed++; // Increment hints counter
            console.log('Hints used:', hintsUsed);
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
    //                      AUTHENTICATION
    //////////////////////////////////////////////////////////////////////////////////
    
    // Update auth status display
    function updateAuthStatus() {
        if (authToken && currentUsername) {
            authStatus.textContent = `Logged in as ${currentUsername}`;
            authStatus.style.color = '#28a745';
        } else {
            authStatus.textContent = 'Not logged in';
            authStatus.style.color = '#6c757d';
        }
    }

    // Register button
    registerBtn.addEventListener('click', async () => {
        const username = usernameInput.value.trim();
        const password = passwordInput.value.trim();
        
        if (!username || !password) {
            alert('Please enter username and password');
            return;
        }

        try {
            const response = await fetch('/api/auth/register', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ username, password })
            });

            if (response.ok) {
                const data = await response.json();
                authToken = data.token;
                refreshToken = data.refreshToken;
                currentUsername = data.username;
                
                localStorage.setItem('authToken', authToken);
                localStorage.setItem('refreshToken', refreshToken);
                localStorage.setItem('username', currentUsername);
                
                updateAuthStatus();
                loadPerformanceStats();
                alert('Registration successful!');
                passwordInput.value = '';
            } else {
                const error = await response.text();
                alert(`Registration failed: ${error}`);
            }
        } catch (err) {
            alert('Registration error: ' + err.message);
        }
    });

    // Login button
    loginBtn.addEventListener('click', async () => {
        const username = usernameInput.value.trim();
        const password = passwordInput.value.trim();
        
        if (!username || !password) {
            alert('Please enter username and password');
            return;
        }

        try {
            const response = await fetch('/api/auth/login', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ username, password })
            });

            if (response.ok) {
                const data = await response.json();
                authToken = data.token;
                refreshToken = data.refreshToken;
                currentUsername = data.username;
                
                localStorage.setItem('authToken', authToken);
                localStorage.setItem('refreshToken', refreshToken);
                localStorage.setItem('username', currentUsername);
                
                updateAuthStatus();
                loadPerformanceStats();
                alert('Login successful!');
                passwordInput.value = '';
            } else {
                alert('Login failed: Invalid username or password');
            }
        } catch (err) {
            alert('Login error: ' + err.message);
        }
    });

    // Logout button
    logoutBtn.addEventListener('click', () => {
        authToken = null;
        refreshToken = null;
        currentUsername = null;
        
        localStorage.removeItem('authToken');
        localStorage.removeItem('refreshToken');
        localStorage.removeItem('username');
        
        usernameInput.value = '';
        passwordInput.value = '';
        updateAuthStatus();
        alert('Logged out successfully!');
    });

    // Initialize auth status on page load
    updateAuthStatus();

    //////////////////////////////////////////////////////////////////////////////////
    //                      START GAME
    //////////////////////////////////////////////////////////////////////////////////
    //Function to start the game
    function startGame(){
        drawGrids();
        stopTimer(); // Don't start timer automatically
        seconds = 0;
        hintsUsed = 0; // Reset hints counter
        showTimer.textContent = formatTime(seconds);
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
    // Set initial blur state
    inputs.style.filter = 'blur(8px)';
    canvas.style.filter = 'blur(8px)';
    inputs.style.pointerEvents = 'none';
    
    // Load stats on page load (general stats if not logged in, personal if logged in)
    loadPerformanceStats();

});