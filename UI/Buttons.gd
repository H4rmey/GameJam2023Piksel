extends Control


func _on_play_button_pressed():
	get_tree().change_scene_to_file("res://Enviroment/World/Scenes/World.tscn")
	pass # Replace with function body.


func _on_lore_button_pressed():
	get_tree().change_scene_to_file("res://UI/Scenes/LoreScreen.tscn")
	pass # Replace with function body.


func _on_credits_button_pressed():
	get_tree().change_scene_to_file("res://UI/Scenes/CreditsScreen.tscn")
	pass # Replace with function body.


func _on_exit_button_pressed():
	get_tree().quit()
	pass # Replace with function body.

func _on_back_button_pressed():
	get_tree().change_scene_to_file("res://UI/Scenes/Start Screen.tscn")
	pass # Replace with function body.

func _on_texture_button_pressed():
	pass # Replace with function body.



