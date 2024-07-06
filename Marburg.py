from popit import *
import csv

csv_file_path = r"./positions_scores.csv"

positions = []
scores = []

# Read positions and scores from CSV
with open(csv_file_path, mode='r') as file:
    reader = csv.reader(file)
    next(reader)  # Skip header row
    for row in reader:
        pos, score = row
        positions.append(pos)
        scores.append(int(score))

def popToFen(board):
  string = ""
  for i in board:
    string += str(i)
  return string

def get_best_move(PopIt):
    rootBestMove = (None, None)
    max = -2
    moves = moveGen(PopIt)
    for index in range(len(moves)):
        for pops in range(1, moves[index] + 1):
            newPop = PopIt.makeMove(index, pops)
            if isCheckMate(newPop):
                return (index, pops)
            newPop.board.sort()
            for i in range(len(positions)):
                if positions[i] == popToFen(newPop.board):
                    score = -scores[i]
                    if score > max:
                        max = score
                        rootBestMove = (index, pops)

    return rootBestMove


def uci():
   global start_time, remaining_time, rootBestMove
   Pop = PopIt()
   while True:
      try:
          line = input()
      except EOFError:
          return
      if line == "upi":
         print("id name Marburg")
         print("id author Dragjon")
         print("upiok")
        
      elif line == "isready":
         print("readyok")

      elif line.startswith("position"):
         posStr = line.split(" ")[1]
         pos, turn = oldStringToArray(posStr)
         Pop = PopIt(pos, turn)

      elif line.startswith("go"):
            bestRow, bestNumPops = get_best_move(Pop)
            print(f"bestmove {bestRow} {bestNumPops}")

uci()
