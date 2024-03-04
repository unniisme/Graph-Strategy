extends Label

func _process(delta):
	var grid = GameManager.GetLevel().grid
	text = str(
		grid.GameCoordinateToGridCoordinate(grid.get_global_mouse_position())
	)
