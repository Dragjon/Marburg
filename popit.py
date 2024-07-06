FIRST = 1
SECOND = 2

class PopIt:

  def __init__(self, board=None, turn=FIRST):
    if board is None:
      self.board = [6 for _ in range(6)]
    else:
      self.board = board
    self.turn = turn

  def makeMove(self, moveRow, numberOfPops):
    new_board = [pops for pops in self.board]
    if new_board[moveRow] >= numberOfPops:
        new_board[moveRow] -= numberOfPops
    new_turn = 3 - self.turn
    return PopIt(board=new_board, turn=new_turn)

def printPopIt(PopIt):
  for row in range(6):
    print(f"{row + 1} ", end="")
    if PopIt.board[row] == 6:
       print("- - - - - -", end=" ")
    elif PopIt.board[row] == 0:
       print("X X X X X X", end=" ")
    else:
        for _ in range(6 - PopIt.board[row]):
           print("X", end=" ")
        for _ in range(PopIt.board[row]):
            print("-", end=" ")
    print()
  print("  1 2 3 4 5 6")
  print()

def moveGen(PopIt):
  return [pops for pops in PopIt.board]

def isCheckMate(PopIt):
  numPops = 0
  for pops in PopIt.board:
    numPops += pops
    if numPops > 1:
        return False
  return True

def oldStringToArray(string):
   array = []
   i = 0
   tmp = 0
   for item in string:
      i += 1
      if i == 37:
        array.append(int(item))
        break
      tmp += 1 if int(item) == 0 else 0
      if i % 6 == 0:
         array.append(tmp)
         tmp = 0
   return array[:6], array[6] 
