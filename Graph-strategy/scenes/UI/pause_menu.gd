extends CanvasLayer

var label : Label

func _ready():
	label = $Label
	$Button.button_up.connect(get_tree().quit)

func player_win(player_index):
	label.text = "Humans\nWon!" if player_index == 0 else "Goblins\nWon!"
	visible = true
	get_tree().paused = true
	
		
