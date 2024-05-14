extends Area2D


func _on_area_entered(area):
	GameManager.GetLevel().EndLevel(1)
