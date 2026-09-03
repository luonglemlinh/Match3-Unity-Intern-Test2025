# TEST: 
## TASK 1: RESKIN
- Re-skin all items into Fish (assets available in the project)
## TASK 2: CHANGE THE GAMEPLAY
- Change the current gameplay to a new one:
  + Move items from the board to the bottom cells by tapping on them.
  + Once an item moves to a bottom cell, you can’t move it back to the board.
  + If there are exactly three identical items in the bottom cells, they will be cleared.
  + Clear the board to win.
  + The player loses if he/she fills up all the bottom cells.

- Requirements:
  + The number of identical items on the initial board must be divisible by 3.
  + The bottom area contains 5 cells.
  + Show a simple winning screen when the player wins.
  + Show a simple losing screen when the player loses.
  + Create a simple Home screen with an ‘Autoplay’ button. Once clicked, the game will autoplay until it wins, with each action having a 0.5s delay.
  + Add an ‘Auto Lose’ button to the Home screen. Once clicked, the game will autoplay with the goal of losing, with each action having a 0.5s delay.
## TASK 3: IMPROVE THE GAMEPLAY
- Ensure the initial board contains all types of fish.
- Add an animation when an item moves from the board to the cells and another animation when identical items are cleared (scaling to 0).
- Add a Time Attack Mode.
  + Add a ‘Time Attack’ button to the Home Screen to play a separate game mode.
  + The player no longer loses the game when the cells are filled.
  + The player can return an item from a cell to its initial position on the board by tapping it.
  + The player loses if they fail to clear the board within 1 minute.

# MY WORK: 

## Note:
If cloning via command line, or having compile errors with DOTween, please run Git LFS to pull the required plugin binaries:
`git lfs pull`

## TASK 1: RESKIN
Assets\Resources\prefabs\itemNormal.prefab > Sprite Renderer > Sprite > fish
## TASK 2: CHANGE THE GAMEPLAY
**IDEA:**
  + Create a tray that holds slots, the number of slots can be configured in settings
  + Change core gameplay loop from Bejeweled to match-3, maybe can keep the old matching mechanic, change Input.GetMouseButton, make a function to move the item to tray
  + Win conditions: cells cleared; create UIPanelWin.cs and win eStateGame
  + add 3 more buttons for AutoPLay, AutoLose, TimeAttack & wire to their corresponding scripts
 
### **CHANGES MADE:**

#### _**Codes:**_

- **GameSettings.cs:** Added public int TraySlotCount
- Created **TraySlotsHolder.cs** using TraySlotCount
- **Board.cs:** Added FillWithTripleCounts(), IsEmpty(), FindCellOfType()
- **BoardController.cs:**
  + Added tray slot variables, Updated StartGame()
  + Modified Update(), Added OnCellTapped(), GetFreeTraySlot(), PlaceInTray
  + Removed m_isDragging, m_timeAfterFill, m_hintIsShown, m_hitCollider, FindMatchesAndCollapse(), ResetRayCast(), ShowHints(), StopHints()
  + OnGameStateChange(): added Win case
  + Added AutoPlayRoutine(), AutoLoseRoutine()  
- **GameManager.cs:** Added ClearLevel(), changed LoadLevel()
- **UIMainManager.cs:** Added Win(), Lose()
- **UIPanelMain:** Added AutoPlay, AutoLose Button with their corresponding functions
- Created **LevelAutoPlay.cs** and **LevelAutoLose.cs** for button wiring
  
#### _**Scene:**_

-  _PanelWin:_ Wired with **UIPanelWin.cs** 
- _PanelHome:_ Wired sripts with their respective gameplay; and wired btn with their respective OnClick() scripts

## TASK 3: IMPROVE THE GAMEPLAY

- **Board.cs**: FillWithTripleCounts() added: every ENormalType is added 3 times initially
- **BoardController.cs:**
  + Added Animation: item.View,DOMove;  ExplodeView() (DOScale(0f, 0.25f) in **Item.cs**
  + Added GetNearTraySlot() and ReturnItemtoBoard() for moving item back to the tray in timer mode 
  

## Setup Instructions
If cloning via command line,or  please run Git LFS to pull the required plugin binaries:
`git lfs pull`

