# a script to run the yolov8 training
import os
from ultralytics import YOLO

# os.system("cd ultralytics-main && "
#         "yolo train data=datasets/FireDataSet/data.yaml model=yolov8n.pt epochs=100 lr0=0.01 batch=5")
if __name__ == '__main__':
    model = YOLO("yolov8n.pt")
    model.train(data="ultralytics-main/datasets/FireDataSet/data.yaml", epochs=100, lr0=0.01, batch=5)
