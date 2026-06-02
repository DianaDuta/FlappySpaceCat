import os
from PIL import Image

base_dir = r"c:\Users\diana\Desktop\PROYECTOS ACTUALES\Flappy_Space_Cat\Assets\Space_Exploration_GUI_Kit\Button_Images"
out_dir = os.path.join(base_dir, "White_Sprites")
os.makedirs(out_dir, exist_ok=True)

all_pngs = []
for root, dirs, files in os.walk(base_dir):
    if "White_Sprites" in root:
        continue
    for f in files:
        if f.endswith(".png"):
            if "-purple-" in f or "-disabled-" in f:
                all_pngs.append(os.path.join(root, f))

count = 0
for path in all_pngs:
    try:
        img = Image.open(path).convert("RGBA")
        data = list(img.getdata())
        new_data = []

        # We want to preserve the perceived luminance but push it towards white.
        # Gamma correction is perfect for this.
        # We will first convert to perceptual grayscale, then apply gamma.
        gamma = 0.4  # values < 1 brighten midtones heavily without clipping highlights
        
        for item in data:
            if item[3] == 0:
                new_data.append((255, 255, 255, 0))
                continue
                
            # perceptual luminance
            l = 0.299 * item[0] + 0.587 * item[1] + 0.114 * item[2]
            
            # apply gamma
            l_norm = l / 255.0
            l_gamma = l_norm ** gamma
            
            final_v = int(l_gamma * 255)
            final_v = max(0, min(255, final_v))
            
            new_data.append((final_v, final_v, final_v, item[3]))
            
        img.putdata(new_data)
        
        name = os.path.basename(path)
        name = name.replace("-purple", "-white").replace("-disabled", "-white-disabled")
        if "-white" not in name:
            name = name.replace(".png", "-white.png")
            
        out_path = os.path.join(out_dir, name)
        img.save(out_path, "PNG")
        count += 1
        
    except Exception as e:
        print(f"Failed {path}: {e}")

print(f"Processed {count} sprites using Gamma 0.4.")
