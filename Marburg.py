import Popitto as pt
import time 

def stmEval(PopIt):
  ''' 
  - If even number of rows with at least 2 empty spaces
    - current player win
    
  - Else
    - not current player wins
    
  - If odd number of rows with at least 2 empty spaces
    - not current player wins
    
  - Else
    - current player wins
  '''

  rows = 0
  numRowsWithAtLeast2EmptySpaces = 0

  for row in range(6):
    numEmptySpaces = 0
    emptyRow = 0
    for col in range(6):
      if PopIt.board[row][col] == pt.NONE and emptyRow == 0:
        emptyRow = 1
        numEmptySpaces += 1
    rows += emptyRow
    if numEmptySpaces >= 2:
      numRowsWithAtLeast2EmptySpaces += 1

  if numRowsWithAtLeast2EmptySpaces != 0 and numRowsWithAtLeast2EmptySpaces % 2 == 0:
    return -1000
  elif numRowsWithAtLeast2EmptySpaces % 2 != 0:
    return 1000
  else:
    if rows % 2 == 0:
      return 1000
    else:
      return -1000
    
def flatten_2d_array(array):
    return [element for row in array for element in row]

rootBestMove = (0, 0)
TT = [(None, None) for _ in range(8388608)]
nodes = 0

class TimeoutException(Exception):
    pass

def negamax(PopIt, depth, ply, alpha, beta):
    global rootBestMove, nodes, remaining_time, hbtm, start_time

    if (depth > 1) and (1000 * (time.time() - start_time) > (remaining_time / hbtm)):
        raise TimeoutException()
    if pt.isCheckMate(PopIt) and ply != 0:
        return -30000 + ply
    if depth == 0:
        return stmEval(PopIt)

    TTHash = hash(tuple(flatten_2d_array(PopIt.board))) % 8388608
    TT_row, TT_pops = TT[TTHash]
    maxScore = -100000
    row = 0
    breakOut = False
    moves = []

    # Generate moves
    for totalPopsAvail in pt.moveGen(PopIt):
        for numberOfPops in range(1, totalPopsAvail + 1):
            moves.append((row, numberOfPops))
        row += 1

    # Order moves
    moves.sort(key=lambda move: (move[0] != TT_row, move[1] != TT_pops))

    for move in moves:
        row, numberOfPops = move
        nodes += 1
        new_PopIt = PopIt.makeMove(row, numberOfPops)
        score = -negamax(new_PopIt, depth - 1, ply + 1, -beta, -alpha)

        if score > maxScore:
            maxScore = score
            TT[TTHash] = (row, numberOfPops)
            if score > alpha:
                alpha = score
                if score >= beta:
                    breakOut = True
                    break
            if ply == 0:
                rootBestMove = (row, numberOfPops)

        if breakOut:
            break

    return maxScore


def get_best_move(PopIt):
    global nodes, rootBestMove, sbtm, remaining_time, start_time
    nodes = 0
    rootBestMove = None
    start_time = time.time()

    try:
        for i in range(1, 256):
            score = negamax(PopIt, i, 0, -100000, 100000)
            elapsed = 1000 * (time.time() - start_time)

            bestRow, bestNumPops = rootBestMove

            print(
                f"info depth {i} score {score} nodes {nodes} time {int(elapsed)} nps {int(1000 * nodes / (elapsed + 0.0000000001))} pv {bestRow} {bestNumPops}"
            )

            if elapsed > (remaining_time / sbtm):
                break
    except TimeoutException as e:
       return

def bench(Pop, maxDepth):
  global nodes, rootBestMove
  nodes = 0
  startTime = time.time()
  for depth in range(1, maxDepth + 1):
    score = negamax(Pop, depth, 0, -100000, 100000)
    elapsed = time.time() - startTime
    bestRow, bestNumPops = rootBestMove
    print(
        f"info depth {depth} score {score} nodes {nodes} time {int(1000 * elapsed)} nps {int(nodes / (elapsed + 0.0000000001))} pv {bestRow} {bestNumPops}"
    )


def parse_parameters(line):
    DEFAULT_TIME1 = 60000
    DEFAULT_TIME2 = 60000
    parameters = line.split()[1:]
    time1, time2 = DEFAULT_TIME1, DEFAULT_TIME2

    for i in range(len(parameters)):
        if parameters[i] == "infinite":
            time1 = float('inf')
            time2 = float('inf')
        elif parameters[i] == "time1" and i + 1 < len(parameters):
            time1 = float(parameters[i + 1])
        elif parameters[i] == "time2" and i + 1 < len(parameters):
            time2 = float(parameters[i + 1])

    return time1, time2



remaining_time = 60000
start_time = None
hbtm = 10
sbtm = 40

def uci():
   global start_time, remaining_time, rootBestMove
   Pop = pt.PopIt()
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
         pos, turn = pt.stringToArray(posStr)
         Pop = pt.PopIt(pos, turn)

      elif line.startswith("go"):
            time1, time2 = parse_parameters(line)
            remaining_time = time1 if Pop.turn == pt.FIRST else time2
            start_time = time.time()
            get_best_move(Pop)
            bestRow, bestNumPops = rootBestMove
            print(f"bestmove {bestRow} {bestNumPops}")
         

uci()
