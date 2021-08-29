import tensorflow as tf
import numpy as np
from matplotlib import pyplot as plt
import cv2
import json
import socket
import time

host, port = "127.0.0.1", 25001
sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
sock.connect((host, port))
startPos = [0, 0, 0]


interpreter = [tf.lite.Interpreter(model_path='lite-model_movenet_singlepose_lightning_3.tflite'),tf.lite.Interpreter(model_path='lite-model_movenet_singlepose_thunder_3.tflite')]
interpreter_selector = 0
interpreter[interpreter_selector].allocate_tensors()


EDGES = {
    (0, 1): 'm',
    (0, 2): 'c',
    (1, 3): 'm',
    (2, 4): 'c',
    (0, 5): 'm',
    (0, 6): 'c',
    (5, 7): 'm',
    (7, 9): 'm',
    (6, 8): 'c',
    (8, 10): 'c',
    (5, 6): 'y',
    (5, 11): 'm',
    (6, 12): 'c',
    (11, 12): 'y',
    (11, 13): 'm',
    (13, 15): 'm',
    (12, 14): 'c',
    (14, 16): 'c'
}

def draw_keypoints(frame, keypoints, confidence_threshold):
    y, x, c = frame.shape
    shaped = np.squeeze(np.multiply(keypoints, [y,x,1]))
    
    for kp in shaped:
        ky, kx, kp_conf = kp
        if kp_conf > confidence_threshold:
            cv2.circle(frame, (int(kx), int(ky)), 4, (0,255,0), -1) 
            


def draw_connections(frame, keypoints, edges, confidence_threshold):
    y, x, c = frame.shape
    shaped = np.squeeze(np.multiply(keypoints, [y,x,1]))
    
    for edge, color in edges.items():
        p1, p2 = edge
        y1, x1, c1 = shaped[p1]
        y2, x2, c2 = shaped[p2]
        
        if (c1 > confidence_threshold) & (c2 > confidence_threshold):      
            cv2.line(frame, (int(x1), int(y1)), (int(x2), int(y2)), (0,0,255), 2)


cap = cv2.VideoCapture(0)
while cap.isOpened():
    # message = socket.recv()
    # print("Received request: %s" % message)
    ret, frame = cap.read()
    
    # Resize image
    img = frame.copy()
    
    if(interpreter_selector==0):
        img = tf.image.resize_with_pad(np.expand_dims(img, axis=0), 192,192)
    elif(interpreter_selector == 1):
        img = tf.image.resize_with_pad(np.expand_dims(img, axis=0), 256,256)
    else:
        print("Select An Appropriate Interpreter")
        
        
    input_image = tf.cast(img, dtype=tf.float32)
    
    # Setup input and output 
    input_details = interpreter[interpreter_selector].get_input_details()
    output_details = interpreter[interpreter_selector].get_output_details()
    
    # Make predictions 
    interpreter[interpreter_selector].set_tensor(input_details[0]['index'], np.array(input_image))
    interpreter[interpreter_selector].invoke()
    keypoints_with_scores = interpreter[interpreter_selector].get_tensor(output_details[0]['index'])
    # print(keypoints_with_scores)
    
    
    lists = keypoints_with_scores.tolist()
    list_1 = lists[0][0]
    
    s = ''.join(str(e) for e in list_1)
    print(s)
    
    sock.sendall(s.encode("UTF-8"))
    receivedData = sock.recv(1024).decode("UTF-8")
    

    # Rendering 
    draw_connections(frame, keypoints_with_scores, EDGES, 0.4)
    draw_keypoints(frame, keypoints_with_scores, 0.4)
    
    cv2.imshow('MoveNet Lightning', frame)
    
    if cv2.waitKey(10) & 0xFF==ord('q'):
        break
        
cap.release()
cv2.destroyAllWindows()

