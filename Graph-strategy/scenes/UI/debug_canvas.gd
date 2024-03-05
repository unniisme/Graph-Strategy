extends CanvasLayer

@export var graph_manager : Node2D

var show_graph_button : Button

func _ready():
	show_graph_button = $ShowGraphButton
	show_graph_button.button_up.connect(graph_manager.Debug)
