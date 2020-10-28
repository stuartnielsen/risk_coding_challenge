# Risk server for the Spring 2021 coding challenge

Play a (simplified) game of Risk with automated bots as players.

### How it works

- Each bot will post to server/join with `{name, callback}` where name is the name of the player and callback is an api base address for server to client calls.
- The game starts when the server receives an authenticated request to `{server}/startgame`
- Placement Phase
  - A post request will be sent to each player at `{callback}/placeArmy` sending `{board, status, armiesRemaining}`
  - The player will respond with the location where they'd like to place an army.
  - If the requested location is invalid, another request will be sent to that same player with status "Placement Failed.  Try another territory."
  - If the requested location is valid (e.g. it is unoccupied or that player occupies the territory), the number of available armies yet to place will be decremented and the next player will be asked to place an army.
  - When all players have placed all armies the game moves to the attacking phase.
- Attacking Phase
  - A post request will be sent to a player at `{callback}/beginAttack` sending `{board, status}`
  - The player will respond with `{fromLocation, toLocation}`
  - The server will validate that the current player owns `fromLocation`, and the current player does *not* own `toLocation`
  - If `toLocation` is unoccupied, the attacker 'wins' and takes possession of the territory.  One army is left in `fromLocation` and all other armies are moved into `toLocation`
  - The server will roll min(armies, 3) attacking dice and min(armies, 2) defending dice.  The attacking and defending dice will be ordered greatest to lest, then each successive pair will be evaluated.  Ties go to the defender.
      For example, if the attacker rolls a 1, 3, 6 and the defender rolls a 3, 5 then the highest dice is 6 attacker, 5 defender - attacker wins (defender loses an army).  The next highest is 3 attacker, 3 defender - defender wins (attacker loses an army).
  - If the last defending army dies, the attacker takes posession of the territory.  One army is left in `fromLocation` and all other armies are moved into `toLocation`
  - If the attacker still has 2 or more armies, a request will be sent to `{callback}/continueAttack` sending `{board}`
  - The player will respond with `{continueAttack: true}` or `{continueAttack: false}`
  - Attacks continue until no army can attack any longer.  You can attack if you have a territory with more than one army adjacent to an enemy or unoccupied territory.
- Game over
  - Once every army has completed every possible attack the game is over.
  - Final score is calculated as armyCount + territoryCount * 2
    

