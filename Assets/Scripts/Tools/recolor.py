import os
from PIL import Image

files = [
    r"c:\Users\diana\Desktop\PROYECTOS ACTUALES\Flappy_Space_Cat\Assets\Space_Exploration_GUI_Kit\Button_Images\Source_Image_Sprites\small\large-purple-small.png",
    r"c:\Users\diana\Desktop\PROYECTOS ACTUALES\Flappy_Space_Cat\Assets\Space_Exploration_GUI_Kit\Button_Images\Pressed_Sprites\Small\large-purple-pressed-small.png",
    r"c:\Users\diana\Desktop\PROYECTOS ACTUALES\Flappy_Space_Cat\Assets\Space_Exploration_GUI_Kit\Button_Images\Disabled_Sprites\Small\large-disabled-small.png",
    r"c:\Users\diana\Desktop\PROYECTOS ACTUALES\Flappy_Space_Cat\Assets\Space_Exploration_GUI_Kit\Button_Images\Highlighted_Sprite\small\large-purple-highlight-small.png"
]

out_dir = r"c:\Users\diana\Desktop\PROYECTOS ACTUALES\Flappy_Space_Cat\Assets\Space_Exploration_GUI_Kit\Button_Images\White_Sprites"
os.makedirs(out_dir, exist_ok=True)

for path in files:
    try:
        img = Image.open(path).convert("RGBA")
        data = img.getdata()
        new_data = []
        
        # We need to find the max brightness to normalize the button so the base becomes white
        max_v = 1
        for item in data:
            if item[3] > 0:
                v = max(item[0], item[1], item[2])
                if v > max_v:
                    max_v = v
                    
        for item in data:
            if item[3] == 0:
                new_data.append((255, 255, 255, 0))
                continue
                
            # Convert to grayscale based on max value to make the base purely white but keep shadows
            # Using max of RGB usually preserves the shadow intensity best for a tinted sprite
            v = max(item[0], item[1], item[2])
            
            # Normalize to 255 if it's not already, meaning the brightest part of purple becomes white
            v_norm = int((v / max_v) * 255)
            
            # Since the button might be very dark purple, let's also preserve some lightness mapping
            # Actually, standard Grayscale is fine: L = 0.299 R + 0.587 G + 0.114 B
            # Let's use simple luminance, then normalize.
            l = int(0.299 * item[0] + 0.587 * item[1] + 0.114 * item[2])
            l_norm = int((l / 255.0) * 255) 
            
            # But the user asked for a BLANCO (White) button. The purple might be a bit dark.
            # Let's artificially brighten it so it's a nice white button.
            # A white button typically has L mostly near 255, and shadows dropping to maybe 150-200.
            # Let's map the current brightness 'l' (0 to l_max) to a brighter range (e.g., 100 to 255).
            # To keep it simple, let's just make the Hue = White (Saturation = 0), and maximize Value (Brightness).
            # So V = max(R, G, B). We normalize V so the brightest pixel is 255.
            # Then to make it "white" instead of "gray", we apply a gamma or just shift it.
            # For now, let's just use the normalized max(R,G,B).
            final_v = min(255, int(v * (255 / max_v) * 1.2)) # boost brightness slightly
            
            new_data.append((final_v, final_v, final_v, item[3]))
            
        img.putdata(new_data)
        
        name = os.path.basename(path).replace("purple", "white")
        out_path = os.path.join(out_dir, name)
        img.save(out_path, "PNG")
        print(f"Saved: {out_path}")
        
    except Exception as e:
        print(f"Failed {path}: {e}")
