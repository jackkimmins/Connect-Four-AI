# Connect Four Solver
What is this project?
 - **Minimax** Algorithm with **α**-**β**-Pruning
 - Transposition Table for Optimal Performance
 - Web-based Game built with [Jindium](https://github.com/jackkimmins/Jindium2)

## What is Connect Four?
Connect Four is a two-player board game in which the players take turns dropping coloured pieces from the top into a grid. The objective of the game is to be the first player to get four of their coloured discs in a row, either horizontally, vertically, or diagonally.

The grid consists of a vertical stack of seven columns and six rows. Players take turns dropping their discs into the top of one of the columns, and the disc falls to the lowest unoccupied row in that column.

The game ends when one of the players gets four of their discs in a row or when the grid is full and there is no winner.

## Algorithmic Theory
A Minimax algorithm is a decision-making algorithm that is commonly used to determine the best move in a two-player game, such as Connect Four. It works by considering all possible moves that a player can make, and then evaluating the potential outcomes of those moves.
This solution also utilises Alpha-beta pruning, which is a technique used to improve the efficiency of the minimax algorithm. It works by eliminating certain branches of the search tree that are unlikely to lead to the best possible move.

To solve connect four with a minimax algorithm and alpha-beta pruning, this algorithm first generates a search tree that represents all the possible moves that could be made in the game. It then evaluates the potential outcomes of each move and uses alpha-beta pruning to eliminate any branches of the tree that are unlikely to lead to the best possible move.

The algorithm then uses the remaining branches of the tree to determine the best move to make, based on the potential outcomes of each move. This would allow the algorithm to make strategic decisions that maximize its chances of winning the game.
