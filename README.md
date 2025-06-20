Semester project in Programming 2 course at Charles University in Prague
by Vasyl Diakulych

Theme:
C# Chess Game implementation, with possibility of playing against AI player

Getting started:
1. Download project
2. Open terminal in folder with project
3. Run ```dotnet run --project ChessUI/ChessUI.csproj```
4. Enjoy the project

How to use an application:
After start you'll see main menu:

![image](https://github.com/user-attachments/assets/53ea99ad-d873-4237-9928-2d3f4db34af9)

- "New Game" button will start game option window, where you can choose opponent(Human player or various chess bots)

- "New Game with custom position"  button will start game option window, where you can choose opponent(Human player or various chess bots) and after choosing an opponent and side, it will suggest you to upload your own .txt file with custom starting position. 
The format of this file will be provided later, also there are few custom positions inside project folder, where you can test your ides.

- "Quit" button will close the application

Technical notes:

UI:
- UI completely made using GTKSharp and Cairo. after each move, application redraws the board
- Assests used are free for use, found on internet 
- You can find source code at SemesterProject/ChessUI/Program.cs

Game itself:
- Logic of the game completely separate of UI
- Source code is located at SemesterProject/ChessLogic/
- Game logic widely uses object-oriented approach, also it is decomposed into different classes and files

AI(file ChessLogic/AIs.cs):
- Currently there are 4 versions of AI, 3 are using heuristic approach and 1 makes random moves(RandomAI, line 31)
- Every line reference below is about newest version of heuristic approach AI(unless specified otherwise). 2 other AIs are just older versions, so the main difference is absence of some functions implemented later. Implementation itself almost the same
- Chess bot uses minimax algorithm with alpha-beta pruning(Minimax() function, line 573)
- It implements different ompitizations like transposition tables and move prioritizing(transposition table dictionary, line 402 and OrderMoves function, line 504)
- For total evaluation, it uses sum of material evaluation, mobility evaluation(number of your moves - number of opponent moves) and position evaluation(Eval fucntion, line 543)
- Position evaluation uses information about current game stage(Oppening, Middlegame, Endgame) and piece position tables, where each piece has prefered squares(line 449, and entire file PiecePosTables.cs)


Format of the starting positon file:
- File should be .txt
- White pieces should be an uppercase letter which corresponds to a first letter of a piece(exception: Knight -> N)
- Black pieces should be lowercase
- Empty tiles should be marked as dot ('.')
- End of the row should be marked with new line character
- Remember, this game is a white-side POW, so put black pawns on top part of the board and white pawns at the bottom
- Example:
  
 ![image](https://github.com/user-attachments/assets/7414be34-21fb-482b-be29-d56f95efc8aa)

