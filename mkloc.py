#!/usr/bin/python3
import sys, os, io
import argparse
import struct
import json
from pathlib import Path

parser = argparse.ArgumentParser(description='Convert an A3X locale from JSON to binary or vice versa.')
parser.add_argument('inFile', help='source file')
parser.add_argument('outFile', nargs='?', help='target .loc or .json file')
args = parser.parse_args()

ext = Path(args.inFile).suffix

if ext == '.json':

	stem = Path(args.inFile).stem
	if not args.outFile:
		args.outFile = stem + '.loc'

	inf = open(args.inFile, "r", encoding="utf-8")
	localeJSON = json.load(inf)
	inf.close()

	bf = io.BytesIO()

	def processString(key, max):
		if not key in localeJSON:
			raise Exception(f"missing key '{key}'.")
		v = localeJSON[key];
		if not isinstance(v, str):
			raise Exception(f"'{key}': '{v}' is not a string value.")
		if len(v) > max:
			raise Exception(f"'{key}': '{v}' is too long, must be at most {max} characters.")
		bf.write(struct.pack(f'={max}s', bytes(v, 'cp1252')))

	def processBool(key):
		if not key in localeJSON:
			raise Exception(f"missing key '{key}'.")
		v = localeJSON[key];
		if not isinstance(v, bool):
			raise Exception(f"'{key}': '{v}' is not a boolean value.")
		bf.write(struct.pack(f'=1b', 1 if v else 0))

	def processArray(key, amount, max):
		if not key in localeJSON:
			raise Exception(f"missing key '{key}'.")
		if not isinstance(localeJSON[key], list):
			raise Exception(f"'{key}' is not an array.")
		if len(localeJSON[key]) != amount:
			raise Exception(f"'{key}' must have {amount} items.")
		s = bytearray()
		for v in localeJSON[key]:
			if not isinstance(v, str):
				raise Exception(f"'{key}': '{v}' is not a string value.")
			v2 = bytes(v, 'cp1252')
			s.extend(v2)
			s.append(0)
		if len(s) > max:
			raise Exception(f"'{key}': '{v}' is too long, must be at most {max} characters, but got {len(s)}.")
		bf.write(struct.pack(f'={max}s', s))

	def processScans(key, amount):
		if not key in localeJSON:
			raise Exception(f"missing key '{key}'.")
		if not isinstance(localeJSON[key], list):
			raise Exception(f"'{key}' is not an array.")
		if len(localeJSON[key]) != amount:
			raise Exception(f"'{key}' must have {amount} items, has {len(localeJSON[key])}")
		for v in localeJSON[key]:
			if isinstance(v, str): 
				if len(v) > 1:
					raise Exception(f"'{key}': '{v}' is too long, must be at most 1 character.")
			elif isinstance(v, int):
				if v > 255:
					raise Exception(f"'{key}': '{v}' is too high, must be at most 255.")
		for v in localeJSON[key]:
			if isinstance(v, str):
				bf.write(struct.pack(f'=1c', bytes(v, 'cp1252')))
			elif isinstance(v, int): 
				bf.write(struct.pack(f'=1B', v))

	processString('identifier', 6);
	processArray('shortWeekdays', 7, 32)
	processArray('shortMonths', 12,64)
	processArray('longWeekdays', 7, 64)
	processArray('longMonths', 12, 106)
	processString('shortDate', 16)
	processString('longDate', 16)
	processString('shortTime', 16)
	processString('longTime', 16)
	processString('thousands', 1)
	processString('decimal', 1)
	processString('currency', 4)
	processBool('currencyAfter')
	processScans('scancodes', 256)

	with open(args.outFile, "wb") as of:
		of.write(bf.getbuffer())

elif ext == '.loc':

	stem = Path(args.inFile).stem
	if not args.outFile:
		args.outFile = stem + '.json'

	inp = open(args.inFile, "rb")
	js = {}

	def processString(key, max):
		s, = struct.unpack(f'={max}s', inp.read(max))
		js[key] = s.decode('cp1252').replace('\x00', '')
	
	def processArray(key, amount, max):
		s, = struct.unpack(f'={max}s', inp.read(max))
		a = list(filter(None, map(lambda x: x.decode('cp1252'), s.split(b'\0'))))
		js[key] = a
	
	def processBool(key):
		s, = struct.unpack(f'=1b', inp.read(1))
		js[key] = s == 1
	
	def processScans(key, amount):
		a = []
		for i in range(amount):
			s, = struct.unpack(f'=1c', inp.read(1))
			if s[0] < 31:
				s = s[0]
			else:
				s = s.decode('cp1252')
			a.append(s)
		js[key] = a
	
	processString('identifier', 6);
	processArray('shortWeekdays', 7, 32)
	processArray('shortMonths', 12, 64)
	processArray('longWeekdays', 7, 64)
	processArray('longMonths', 12, 106)
	processString('shortDate', 16)
	processString('longDate', 16)
	processString('shortTime', 16)
	processString('longTime', 16)
	processString('thousands', 1)
	processString('decimal', 1)
	processString('currency', 4)
	processBool('currencyAfter')
	processScans('scancodes', 256)
	
	with open(args.outFile, "w") as of:
		json.dump(js, of)
