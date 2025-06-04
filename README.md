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
- Game logic widely uses object-oriented approach, also it implements decomposition into different classes and even files

AI:
- Chess bot uses minimax algorithm with alpha-beta pruning
- It implements different ompitizations like transposition tables and move prioritizing
- For total evaluation, it uses sum of material evaluation, mobility evaluation(number of your moves - number of opponent moves) and position evaluation
- Position evaluation uses information about current game stage(Oppening, Middlegame, Endgame) and piece position tables, where each piece has prefered squares 


Format of the starting positon file:
- File should be .txt
- White pieces should be an uppercase letter which corresponds to a piece
- Black pieces should be lowercase
- Empty tiles should be marked as dot ('.')
- Example:
  rnbqkbnr
  pppppppp
  ........
  ........
  ........
  ........      
  PPPPPPPP
  RNBQKBNR
