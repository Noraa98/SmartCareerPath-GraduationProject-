import hashlib, base64, binascii
from hashlib import pbkdf2_hmac

password = b'Ahmed@246810'
salt_b64 = 'i2IM/dGD3hOpcWVg43VciQ=='   # from your DB
salt = base64.b64decode(salt_b64)
target_b64 = 'N0+1Vy2vK/DBoBOfFL0l50BbvP3/a26M3uSkd460Sog='  # from your DB

iterations_list = [1000, 2000, 5000, 10000, 20000, 50000, 100000]
algs = [('sha1', 20), ('sha256', 32), ('sha512', 64)]  # (name, expected dklen)
found = False

print("Salt (hex):", binascii.hexlify(salt).decode())
print("Target hash (base64):", target_b64)
print()

for alg, dklen_guess in algs:
    for iters in iterations_list:
        # try a couple of dklen values (expected and 32)
        for dklen in (dklen_guess, 32):
            dk = pbkdf2_hmac(alg, password, salt, iters, dklen)
            out_b64 = base64.b64encode(dk).decode()
            if out_b64 == target_b64:
                print(f"MATCH! alg={alg}, iterations={iters}, dklen={dklen}, out={out_b64}")
                found = True
            else:
                print(f"try alg={alg}, iters={iters}, dklen={dklen} -> {out_b64}")
    print()
if not found:
    print("No exact match found with the tried algorithms/iterations/dklen values. You can extend the iterations list or paste the Auth/Password service source for exact logic.")
